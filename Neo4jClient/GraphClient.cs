using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;
using Neo4jClient.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient
{
    public class GraphClient : IRawGraphClient, IInternalTransactionalGraphClient<HttpResponseMessage>, IDisposable
    {
        internal const string GremlinPluginUnavailable =
            "You're attempting to execute a Gremlin query, however the server instance you are connected to does not have the Gremlin plugin loaded. If you've recently upgraded to Neo4j 2.0, you'll need to be aware that Gremlin no longer ships as part of the normal Neo4j distribution.  Please move to equivalent (but much more powerful and readable!) Cypher.";
        internal const string MaxExecutionTimeHeaderKey = "max-execution-time";

        public static readonly JsonConverter[] DefaultJsonConverters =
        {
            new TypeConverterBasedJsonConverter(),
            new NullableEnumValueConverter(),
            new TimeZoneInfoConverter(),
            new EnumValueConverter()
        };

        public static readonly DefaultContractResolver DefaultJsonContractResolver = new DefaultContractResolver();

        private ITransactionManager<HttpResponseMessage> transactionManager;
        private readonly IExecutionPolicyFactory policyFactory;

        public ExecutionConfiguration ExecutionConfiguration { get; private set; }

        internal readonly Uri RootUri;
        internal RootApiResponse RootApiResponse;
        private RootNode rootNode;

        private CypherCapabilities cypherCapabilities = CypherCapabilities.Default;


        public bool UseJsonStreamingIfAvailable { get; set; }

        //        public GraphClient(Uri rootUri, string username = null, string password = null)
        //            : this(rootUri, new HttpClientWrapper(username, password))
        //        {
        //            ServicePointManager.Expect100Continue = true;
        //            ServicePointManager.UseNagleAlgorithm = false;
        //        }
        //
        //        public GraphClient(Uri rootUri, bool expect100Continue, bool useNagleAlgorithm, string username = null, string password = null)
        //            : this(rootUri, new HttpClientWrapper(username, password))
        //        {
        //            ServicePointManager.Expect100Continue = expect100Continue;
        //            ServicePointManager.UseNagleAlgorithm = useNagleAlgorithm;
        //        }
        
        public GraphClient(Uri rootUri, string username = null, string password = null)
            : this(rootUri, new HttpClientWrapper(username, password))
        {
//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.UseNagleAlgorithm = false;
        }

        public GraphClient(Uri rootUri, bool expect100Continue, bool useNagleAlgorithm, string username = null, string password = null)
            : this(rootUri, new HttpClientWrapper(username, password))
        {
//            ServicePointManager.Expect100Continue = expect100Continue;
//            ServicePointManager.UseNagleAlgorithm = useNagleAlgorithm;
        }

        public virtual async Task ConnectAsync(NeoServerConfiguration configuration = null)
        {
            if (IsConnected)
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var operationCompletedArgs = new OperationCompletedEventArgs
            {
                QueryText = "Connect",
                ResourcesReturned = 0
            };

            Action stopTimerAndNotifyCompleted = () =>
            {
                stopwatch.Stop();
                operationCompletedArgs.TimeTaken = stopwatch.Elapsed;
                OnOperationCompleted(operationCompletedArgs);
            };

            try
            {
                configuration = configuration ?? await NeoServerConfiguration.GetConfigurationAsync(
                                    RootUri,
                                    ExecutionConfiguration.Username,
                                    ExecutionConfiguration.Password,
                                    ExecutionConfiguration.Realm,
                                    ExecutionConfiguration).ConfigureAwait(false);

                RootApiResponse = configuration.ApiConfig;

                if (!string.IsNullOrWhiteSpace(RootApiResponse.Transaction))
                {
                    transactionManager = new TransactionManager(this);
                }

                rootNode = string.IsNullOrEmpty(RootApiResponse.ReferenceNode)
                    ? null
                    : new RootNode(long.Parse(GetLastPathSegment(RootApiResponse.ReferenceNode)), this);

                // http://blog.neo4j.org/2012/04/streaming-rest-api-interview-with.html
                ExecutionConfiguration.UseJsonStreaming = ExecutionConfiguration.UseJsonStreaming &&
                                                          RootApiResponse.Version >= new Version(1, 8);

                if (RootApiResponse.Version < new Version(2, 0))
                    cypherCapabilities = CypherCapabilities.Cypher19;

                if (RootApiResponse.Version >= new Version(2, 2))
                    cypherCapabilities = CypherCapabilities.Cypher22;

                if (RootApiResponse.Version >= new Version(2, 2, 6))
                    cypherCapabilities = CypherCapabilities.Cypher226;

                if (RootApiResponse.Version >= new Version(2, 3))
                    cypherCapabilities = CypherCapabilities.Cypher23;

                if (RootApiResponse.Version >= new Version(3, 0))
                    cypherCapabilities = CypherCapabilities.Cypher30;
            }
            catch (AggregateException ex)
            {
                Exception unwrappedException;
                var wasUnwrapped = ex.TryUnwrap(out unwrappedException);
                operationCompletedArgs.Exception = wasUnwrapped ? unwrappedException : ex;

                stopTimerAndNotifyCompleted();

                if (wasUnwrapped)
                    throw unwrappedException;

                throw;
            }
            catch (Exception e)
            {
                operationCompletedArgs.Exception = e;
                stopTimerAndNotifyCompleted();
                throw;
            }

            stopTimerAndNotifyCompleted();
        }

        public GraphClient(Uri rootUri, IHttpClient httpClient)
        {
            RootUri = rootUri;
            JsonConverters = new List<JsonConverter>();
            JsonConverters.AddRange(DefaultJsonConverters);
            JsonContractResolver = DefaultJsonContractResolver;
            ExecutionConfiguration = new ExecutionConfiguration
            {
                HttpClient = httpClient,
                UserAgent = $"Neo4jClient/{GetType().GetTypeInfo().Assembly.GetName().Version}",
                UseJsonStreaming = true,
                JsonConverters = JsonConverters,
                Username = httpClient?.Username,
                Password = httpClient?.Password
            };
            UseJsonStreamingIfAvailable = true;
            policyFactory = new ExecutionPolicyFactory(this);
        }

        private Uri BuildUri(string relativeUri)
        {
            var baseUri = RootUri;
            if (!RootUri.AbsoluteUri.EndsWith("/"))
                baseUri = new Uri(RootUri.AbsoluteUri + "/");

            if (relativeUri.StartsWith("/"))
                relativeUri = relativeUri.Substring(1);

            return new Uri(baseUri, relativeUri);
        }

        private string SerializeAsJson(object contents)
        {
            return Serializer.Serialize(contents);
        }

        public virtual bool IsConnected => RootApiResponse != null;

        [Obsolete(
            "The concept of a single root node has being dropped in Neo4j 2.0. Use an alternate strategy for having known reference points in the graph, such as labels."
            )]
        public virtual RootNode RootNode
        {
            get
            {
                CheckRoot();
                return rootNode;
            }
        }

        CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer { JsonConverters = JsonConverters, JsonContractResolver = JsonContractResolver };
        }

        public ISerializer Serializer => new CustomJsonSerializer { JsonConverters = JsonConverters, JsonContractResolver = JsonContractResolver };

        private static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }

        public ICypherFluentQuery Cypher => new CypherFluentQuery(this);

        private const string DefaultDatabase = "neo4j";

        public Version ServerVersion
        {
            get
            {
                CheckRoot();
                return RootApiResponse.Version;
            }
        }

        public Uri RootEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri("");
            }
        }

        public Uri TransactionEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.Transaction);
            }
        }

        public Uri CypherEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.Cypher);
            }
        }

        public List<JsonConverter> JsonConverters { get; }

        private void CheckTransactionEnvironmentWithPolicy(IExecutionPolicy policy)
        {
            bool inTransaction = InTransaction;

            if (inTransaction && policy.TransactionExecutionPolicy == TransactionExecutionPolicy.Denied)
            {
                throw new InvalidOperationException("Cannot be done inside a transaction scope.");
            }

            if (!inTransaction && policy.TransactionExecutionPolicy == TransactionExecutionPolicy.Required)
            {
                throw new InvalidOperationException("Cannot be done outside a transaction scope.");
            }
        }

        public ITransaction BeginTransaction()
        {
            return BeginTransaction((IEnumerable<string>) null);
        }

        public ITransaction BeginTransaction(string bookmark)
        {
            return BeginTransaction(new List<string> {bookmark});
        }

        public ITransaction BeginTransaction(IEnumerable<string> bookmarks)
        {
            return BeginTransaction(TransactionScopeOption.Join, bookmarks);
        }

        public ITransaction BeginTransaction(TransactionScopeOption scopeOption)
        {
            return BeginTransaction(scopeOption, (IEnumerable<string>) null);
        }

        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, string bookmark)
        {
            return BeginTransaction(scopeOption, new List<string>{bookmark});
        }

        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, IEnumerable<string> bookmarks)
        {
            CheckRoot();
            if (transactionManager == null)
            {
                throw new NotSupportedException("HTTP Transactions are only supported on Neo4j 2.0 and newer.");
            }

            return transactionManager.BeginTransaction(scopeOption, bookmarks);
        }

        public ITransaction Transaction => transactionManager?.CurrentNonDtcTransaction;

        public bool InTransaction => transactionManager != null && transactionManager.InTransaction;

        public void EndTransaction()
        {
            if (transactionManager == null)
            {
                throw new NotSupportedException("HTTP Transactions are only supported on Neo4j 2.0 and newer.");
            }
            transactionManager.EndTransaction();
        }

        public CypherCapabilities CypherCapabilities => cypherCapabilities;
        
        private async Task<CypherPartialResult> PrepareCypherRequest<TResult>(CypherQuery query, IExecutionPolicy policy)
        {
            if (InTransaction)
            {
                var response = await transactionManager
                    .EnqueueCypherRequest(string.Format("The query was: {0}", query.QueryText), this, query)
                    .ConfigureAwait(false);
                
                var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode, query.ResultFormat, true);
                return new CypherPartialResult
                {
                    DeserializationContext =
                        deserializer.CheckForErrorsInTransactionResponse(await response.Content.ReadAsStringAsync().ConfigureAwait(false)),
                    ResponseObject = response
                };
            }

            int? maxExecutionTime = null;
            NameValueCollection customHeaders = null;
            if (query != null)
            {
                maxExecutionTime = query.MaxExecutionTime;
                customHeaders = query.CustomHeaders;
            }

            return await Request.With(ExecutionConfiguration, customHeaders, maxExecutionTime)
                .Post(policy.BaseEndpoint(query?.Database))
                .WithJsonContent(policy.SerializeRequest(query))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ExecuteAsync(response => new CypherPartialResult
                {
                    ResponseObject = response
                }).ConfigureAwait(false);
        }

        async Task<IEnumerable<TResult>> IRawGraphClient.ExecuteGetCypherResultsAsync<TResult>(CypherQuery query)
        {
            var context = ExecutionContext.Begin(this);
            List<TResult> results;
            try
            {
                // the transaction handling is handled by a thread-local variable (ThreadStatic) so we need
                // to know if we are in a transaction right now because our deserializer will run in another thread
                bool inTransaction = InTransaction;

                var response = await PrepareCypherRequest<TResult>(query, context.Policy).ConfigureAwait(false);
                var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode, query.ResultFormat,
                    inTransaction);
                if (inTransaction)
                {
                    response.DeserializationContext.DeserializationContext.JsonContractResolver =
                        query.JsonContractResolver;
                    results =
                        deserializer.DeserializeFromTransactionPartialContext(response.DeserializationContext).ToList();
                }
                else
                {
                    results = deserializer.Deserialize(await response.ResponseObject.Content.ReadAsStringAsync().ConfigureAwait(false)).ToList();
                }
            }
            catch (AggregateException aggregateException)
            {
                Exception unwrappedException;
                if (aggregateException.TryUnwrap(out unwrappedException))
                {
                    context.Complete(query, unwrappedException);
                }
                else
                {
                    context.Complete(query, aggregateException);
                }
                throw;
            }
            catch (Exception e)
            {
                context.Complete(query, e);
                throw;
            }

            context.Complete(query, results.Count());

            return results;
        }

        async Task IRawGraphClient.ExecuteCypherAsync(CypherQuery query)
        {
            var context = ExecutionContext.Begin(this);

            CypherPartialResult response;
            try
            {
                response = await PrepareCypherRequest<object>(query, context.Policy).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (InTransaction)
                    ExecutionConfiguration.HasErrors = true;
                
                context.Complete(query, e);
                throw e;
            }
            context.Policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(response.ResponseObject), null);

            context.Complete(query);
        }

        private void CheckRoot()
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException(
                    "The graph client is not connected to the server. Call the Connect method first.");
        }


        public event OperationCompletedEventHandler OperationCompleted;

        protected void OnOperationCompleted(OperationCompletedEventArgs args)
        {
            var eventInstance = OperationCompleted;
            if (eventInstance != null)
                eventInstance(this, args);
        }

        private void EnsureNodeWasCreated(BatchStepResult createResponse)
        {
            if (createResponse.Status == HttpStatusCode.BadRequest && createResponse.Body != null)
            {
                var exceptionResponse = JsonConvert.DeserializeObject<ExceptionResponse>(createResponse.Body);

                if (exceptionResponse == null || string.IsNullOrEmpty(exceptionResponse.Message) || string.IsNullOrEmpty(exceptionResponse.Exception))
                    throw new Exception(string.Format("Response from Neo4J: {0}", createResponse.Body));

                throw new NeoException(exceptionResponse);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (transactionManager != null)
                transactionManager.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public DefaultContractResolver JsonContractResolver { get; set; }

        public ITransactionManager<HttpResponseMessage> TransactionManager => transactionManager;

#region ExecutionContext class
        private class ExecutionContext
        {
            private GraphClient owner;

            private readonly Stopwatch stopwatch;

            public IExecutionPolicy Policy { get; set; }
            public static bool HasErrors { get; set; }

            private ExecutionContext()
            {
                stopwatch = Stopwatch.StartNew();
            }

            public static ExecutionContext Begin(GraphClient owner)
            {
                owner.CheckRoot();
                var policy = owner.policyFactory.GetPolicy(PolicyType.Cypher);

                owner.CheckTransactionEnvironmentWithPolicy(policy);

                var executionContext = new ExecutionContext
                {
                    owner = owner,
                    Policy = policy
                };

                return executionContext;
            }

            public void Complete(CypherQuery query)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, 0, null);
            }

            public void Complete(CypherQuery query, int resultsCount)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, resultsCount, null, query.CustomHeaders);
            }

            public void Complete(CypherQuery query, Exception exception)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, -1, exception);
            }

            public void Complete(string queryText, int resultsCount = -1, Exception exception = null, NameValueCollection customHeaders = null, int? maxExecutionTime = null)
            {
                var args = new OperationCompletedEventArgs
                {
                    QueryText = queryText,
                    ResourcesReturned = resultsCount,
                    TimeTaken = stopwatch.Elapsed,
                    Exception = exception,
                    CustomHeaders = customHeaders,
                    MaxExecutionTime = maxExecutionTime
                };

                owner.OnOperationCompleted(args);
            }
        }

#endregion
    }
}
