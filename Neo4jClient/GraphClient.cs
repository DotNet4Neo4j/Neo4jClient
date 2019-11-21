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

        public virtual async Task<NodeReference<TNode>> CreateAsync<TNode>(
            TNode node,
            IEnumerable<IRelationshipAllowingParticipantNode<TNode>> relationships,
            IEnumerable<IndexEntry> indexEntries)
            where TNode : class
        {
            if (typeof (TNode).GetTypeInfo().IsGenericType &&
                typeof (TNode).GetGenericTypeDefinition() == typeof (Node<>))
            {
                throw new ArgumentException(string.Format(
                    "You're trying to pass in a Node<{0}> instance. Just pass the {0} instance instead.",
                    typeof (TNode).GetGenericArguments()[0].Name),
                    "node");
            }

            if (node == null)
                throw new ArgumentNullException("node");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            relationships = relationships ?? Enumerable.Empty<IRelationshipAllowingParticipantNode<TNode>>();
            indexEntries = (indexEntries ?? Enumerable.Empty<IndexEntry>()).ToArray();

            var validationContext = new ValidationContext(node, null, null);
            Validator.ValidateObject(node, validationContext);

            var calculatedRelationships = relationships
                .Cast<Relationship>()
                .Select(r => new
                {
                    CalculatedDirection = Relationship.DetermineRelationshipDirection(typeof(TNode), r),
                    Relationship = r
                })
                .ToArray();

            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Batch);
            CheckTransactionEnvironmentWithPolicy(policy);

            var batchSteps = new List<BatchStep>();

            var createNodeStep = batchSteps.Add(HttpMethod.Post, "/node", node);

            foreach (var relationship in calculatedRelationships)
            {
                var participants = new[]
                {
                    string.Format("{{{0}}}", createNodeStep.Id),
                    string.Format("/node/{0}", relationship.Relationship.OtherNode.Id)
                };
                string sourceNode, targetNode;
                switch (relationship.CalculatedDirection)
                {
                    case RelationshipDirection.Outgoing:
                        sourceNode = participants[0];
                        targetNode = participants[1];
                        break;
                    case RelationshipDirection.Incoming:
                        sourceNode = participants[1];
                        targetNode = participants[0];
                        break;
                    default:
                        throw new NotSupportedException(string.Format(
                            "The specified relationship direction is not supported: {0}",
                            relationship.CalculatedDirection));
                }

                var relationshipTemplate = new RelationshipTemplate
                {
                    To = targetNode,
                    Data = relationship.Relationship.Data,
                    Type = relationship.Relationship.RelationshipTypeKey
                };
                batchSteps.Add(HttpMethod.Post, sourceNode + "/relationships", relationshipTemplate);
            }

            var entries = indexEntries
                .SelectMany(i => i
                    .KeyValues
                    .Select(kv => new
                    {
                        IndexAddress = BuildRelativeIndexAddress(i.Name, IndexFor.Node),
                        kv.Key,
                        Value = EncodeIndexValue(kv.Value)
                    })
                    .Where(e => !string.IsNullOrEmpty(e.Value)));
            foreach (var indexEntry in entries)
            {
                batchSteps.Add(HttpMethod.Post, indexEntry.IndexAddress, new
                {
                    key = indexEntry.Key,
                    value = indexEntry.Value,
                    uri = "{0}"
                });
            }

            var batchResponse = await ExecuteBatch(batchSteps, policy).ConfigureAwait(false);
            var createResponse = batchResponse[createNodeStep];
            EnsureNodeWasCreated(createResponse);
            var nodeId = long.Parse(GetLastPathSegment(createResponse.Location));
            var nodeReference = new NodeReference<TNode>(nodeId, this);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = string.Format("Create<{0}>", typeof(TNode).Name),
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });

            return nodeReference;
        }

        private Task<BatchResponse> ExecuteBatch(List<BatchStep> batchSteps, IExecutionPolicy policy)
        {
            return Request.With(ExecutionConfiguration)
                .Post(policy.BaseEndpoint)
                .WithJsonContent(SerializeAsJson(batchSteps))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<BatchResponse>()
                .ExecuteAsync();
        }

        public virtual Task<RelationshipReference> CreateRelationshipAsync<TSourceNode, TRelationship>(
            NodeReference<TSourceNode> sourceNodeReference,
            TRelationship relationship)
            where TRelationship :
                Relationship,
                IRelationshipAllowingSourceNode<TSourceNode>
        {
            if (sourceNodeReference == null)
                throw new ArgumentNullException("sourceNodeReference");

            if (relationship.Direction == RelationshipDirection.Incoming)
                throw new NotSupportedException("Incoming relationships are not yet supported by this method.");

            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            return CreateRelationshipAsync(
                sourceNodeReference,
                relationship.OtherNode,
                relationship.RelationshipTypeKey,
                relationship.Data,
                policy);
        }

        private async Task<RelationshipReference> CreateRelationshipAsync(NodeReference sourceNode, NodeReference targetNode,
            string relationshipTypeKey, object data, IExecutionPolicy policy)
        {
            var relationship = new RelationshipTemplate
            {
                To = policy.BaseEndpoint.AddPath(targetNode, policy).ToString(),
                Data = data,
                Type = relationshipTypeKey
            };

            var sourceNodeEndpoint = policy.BaseEndpoint
                .AddPath(sourceNode, policy)
                .AddPath("relationships");

            return (await Request.With(ExecutionConfiguration)
                .Post(sourceNodeEndpoint)
                .WithJsonContent(SerializeAsJson(relationship))
                .WithExpectedStatusCodes(HttpStatusCode.Created, HttpStatusCode.NotFound)
                .ParseAs<RelationshipApiResponse<object>>()
                .FailOnCondition(responseMessage => responseMessage.StatusCode == HttpStatusCode.NotFound)
                .WithError(responseMessage => new Exception(string.Format(
                    "One of the nodes referenced in the relationship could not be found. Referenced nodes were {0} and {1}.",
                    sourceNode.Id,
                    targetNode.Id))
                )
                .ExecuteAsync().ConfigureAwait(false))
                //.ReadAsJson<RelationshipApiResponse<object>>(JsonConverters,JsonContractResolver)
                .ToRelationshipReference(this);
        }

        CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer { JsonConverters = JsonConverters, JsonContractResolver = JsonContractResolver };
        }

        public ISerializer Serializer => new CustomJsonSerializer { JsonConverters = JsonConverters, JsonContractResolver = JsonContractResolver };

        public async Task DeleteRelationshipAsync(RelationshipReference reference)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Request.With(ExecutionConfiguration)
                .Delete(policy.BaseEndpoint.AddPath(reference, policy))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent, HttpStatusCode.NotFound)
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.NotFound)
                .WithError(response => new Exception(string.Format(
                    "Unable to delete the relationship. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.ReasonPhrase)))
                .ExecuteAsync().ConfigureAwait(false);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = "Delete Relationship " + reference.Id,
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        public virtual Task<Node<TNode>> GetAsync<TNode>(NodeReference reference)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            return Request.With(ExecutionConfiguration)
                .Get(policy.BaseEndpoint.AddPath(reference, policy))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NotFound)
                .ParseAs<NodeApiResponse<TNode>>()
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.NotFound)
                .WithDefault()
                .ExecuteAsync(nodeMessage => nodeMessage?.ToNode(this));
        }

        public virtual Task<Node<TNode>> GetAsync<TNode>(NodeReference<TNode> reference)
        {
            return GetAsync<TNode>((NodeReference)reference);
        }

        public virtual Task<RelationshipInstance<TData>> GetAsync<TData>(RelationshipReference<TData> reference)
            where TData : class, new()
        {
            return GetAsync<TData>((RelationshipReference)reference);
        }

        public virtual Task<RelationshipInstance<TData>> GetAsync<TData>(RelationshipReference reference) where TData : class, new()
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            return Request.With(ExecutionConfiguration)
                .Get(policy.BaseEndpoint.AddPath(reference, policy))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NotFound)
                .ParseAs<RelationshipApiResponse<TData>>()
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.NotFound)
                .WithDefault()
                .ExecuteAsync(
                    responseTask =>
                        responseTask?.ToRelationshipInstance(this));
        }

        public async Task UpdateAsync<TNode>(NodeReference<TNode> nodeReference, TNode replacementData,
            IEnumerable<IndexEntry> indexEntries = null)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var allIndexEntries = indexEntries == null
                ? new IndexEntry[0]
                : indexEntries.ToArray();

            await Request.With(ExecutionConfiguration)
                .Put(policy.BaseEndpoint.AddPath(nodeReference, policy).AddPath("properties"))
                .WithJsonContent(SerializeAsJson(replacementData))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .ExecuteAsync().ConfigureAwait(false);

            if (allIndexEntries.Any())
                await ReIndexAsync(nodeReference, allIndexEntries).ConfigureAwait(false);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = string.Format("Update<{0}> {1}", typeof(TNode).Name, nodeReference.Id),
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        public async Task<Node<TNode>> UpdateAsync<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback,
            Func<TNode, IEnumerable<IndexEntry>> indexEntriesCallback = null,
            Action<IEnumerable<FieldChange>> changeCallback = null)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var node = await GetAsync(nodeReference).ConfigureAwait(false);

            var indexEntries = new IndexEntry[] { };

            if (indexEntriesCallback != null)
            {
                indexEntries = indexEntriesCallback(node.Data).ToArray();
            }

            var serializer = Serializer;

            var originalValuesString = changeCallback == null ? null : serializer.Serialize(node.Data);

            updateCallback(node.Data);

            if (changeCallback != null)
            {
                var originalValuesDictionary =
                    new CustomJsonDeserializer(JsonConverters, resolver: JsonContractResolver).Deserialize<Dictionary<string, string>>(
                        originalValuesString);
                var newValuesString = serializer.Serialize(node.Data);
                var newValuesDictionary =
                    new CustomJsonDeserializer(JsonConverters, resolver: JsonContractResolver).Deserialize<Dictionary<string, string>>(newValuesString);
                var differences = Utilities.GetDifferencesBetweenDictionaries(originalValuesDictionary,
                    newValuesDictionary);
                changeCallback(differences);
            }

            await Request.With(ExecutionConfiguration)
                .Put(policy.BaseEndpoint.AddPath(nodeReference, policy).AddPath("properties"))
                .WithJsonContent(serializer.Serialize(node.Data))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .ExecuteAsync().ConfigureAwait(false);

            if (indexEntriesCallback != null)
            {
                await ReIndexAsync(node.Reference, indexEntries).ConfigureAwait(false);
            }

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = string.Format("Update<{0}> {1}", typeof(TNode).Name, nodeReference.Id),
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });

            return node;
        }

        public async Task UpdateAsync<TRelationshipData>(RelationshipReference<TRelationshipData> relationshipReference,
            Action<TRelationshipData> updateCallback)
            where TRelationshipData : class, new()
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var propertiesEndpoint = policy.BaseEndpoint.AddPath(relationshipReference, policy).AddPath("properties");
            var currentData = await Request.With(ExecutionConfiguration)
                .Get(propertiesEndpoint)
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NoContent)
                .ParseAs<TRelationshipData>()
                .ExecuteAsync().ConfigureAwait(false);

            var payload = currentData ?? new TRelationshipData();
            updateCallback(payload);

            await Request.With(ExecutionConfiguration)
                .Put(propertiesEndpoint)
                .WithJsonContent(SerializeAsJson(payload))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .ExecuteAsync().ConfigureAwait(false);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = string.Format("Update<{0}> {1}", typeof(TRelationshipData).Name, relationshipReference.Id),
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        public virtual async Task DeleteAsync(NodeReference reference, DeleteMode mode)
        {
            CheckRoot();
            var policy = policyFactory.GetPolicy(PolicyType.Rest);
            CheckTransactionEnvironmentWithPolicy(policy);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (mode == DeleteMode.NodeAndRelationships)
            {
                await DeleteAllRelationships(reference, policy).ConfigureAwait(false);
            }

            await Request.With(ExecutionConfiguration)
                .Delete(policy.BaseEndpoint.AddPath(reference, policy))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent, HttpStatusCode.Conflict)
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.Conflict)
                .WithError(response => new Exception(string.Format(
                    "Unable to delete the node. The node may still have relationships. The response status was: {0} {1}",
                    (int)response.StatusCode,
                    response.ReasonPhrase)))
                .ExecuteAsync().ConfigureAwait(false);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = "Delete " + reference.Id,
                ResourcesReturned = 0,
                TimeTaken = stopwatch.Elapsed
            });
        }

        private async Task DeleteAllRelationships(NodeReference reference, IExecutionPolicy policy)
        {
            //TODO: Make this a dynamic endpoint resolution
            var relationshipEndpoint = policy.BaseEndpoint
                .AddPath(reference, policy)
                .AddPath("relationships")
                .AddPath("all");
            var result = await Request.With(ExecutionConfiguration)
                .Get(relationshipEndpoint)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<RelationshipApiResponse<object>>>()
                .ExecuteAsync().ConfigureAwait(false);

            var relationshipResources = result.Select(r => r.Self);
            foreach (var relationshipResource in relationshipResources)
            {
                await Request.With(ExecutionConfiguration)
                    .Delete(new Uri(relationshipResource))
                    .WithExpectedStatusCodes(HttpStatusCode.NoContent, HttpStatusCode.NotFound)
                    .ExecuteAsync().ConfigureAwait(false);
            }
        }

        private static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }

        public ICypherFluentQuery Cypher
        {
            get { return new CypherFluentQuery(this); }
        }

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

        public Uri BatchEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.Batch);
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

        public Uri RelationshipIndexEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.RelationshipIndex);
            }
        }

        public Uri NodeIndexEndpoint
        {
            get
            {
                CheckRoot();
                return BuildUri(RootApiResponse.NodeIndex);
            }
        }

        public Uri GremlinEndpoint
        {
            get
            {
                CheckRoot();
                if (RootApiResponse.Extensions.GremlinPlugin == null ||
                    string.IsNullOrEmpty(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript))
                {
                    return null;
                }
                return BuildUri(RootApiResponse.Extensions.GremlinPlugin.ExecuteScript);
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
                .Post(policy.BaseEndpoint)
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

        private IExecutionPolicy GetPolicyForIndex(IndexFor indexFor)
        {
            switch (indexFor)
            {
                case IndexFor.Node:
                    return policyFactory.GetPolicy(PolicyType.NodeIndex);
                case IndexFor.Relationship:
                    return policyFactory.GetPolicy(PolicyType.RelationshipIndex);
                default:
                    throw new NotSupportedException(string.Format("GetIndexes does not support indexfor {0}", indexFor));
            }
        }

        private Uri GetUriForIndexType(IndexFor indexFor)
        {
            var policy = GetPolicyForIndex(indexFor);
            CheckTransactionEnvironmentWithPolicy(policy);
            return policy.BaseEndpoint;
        }

        public async Task<Dictionary<string, IndexMetaData>> GetIndexesAsync(IndexFor indexFor)
        {
            CheckRoot();

            var result = await Request.With(ExecutionConfiguration)
                .Get(GetUriForIndexType(indexFor))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NoContent)
                .ParseAs<Dictionary<string, IndexMetaData>>()
                .FailOnCondition(response => response.StatusCode == HttpStatusCode.NoContent)
                .WithDefault()
                .ExecuteAsync().ConfigureAwait(false);

            return result ?? new Dictionary<string, IndexMetaData>();
        }

        public async Task<bool> CheckIndexExistsAsync(string indexName, IndexFor indexFor)
        {
            CheckRoot();

            var baseEndpoint = GetUriForIndexType(indexFor);
            var response = await Request.With(ExecutionConfiguration)
                .Get(baseEndpoint.AddPath(indexName))
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NotFound)
                .ExecuteAsync().ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        private void CheckRoot()
        {
            if (RootApiResponse == null)
                throw new InvalidOperationException(
                    "The graph client is not connected to the server. Call the Connect method first.");
        }

        public Task CreateIndexAsync(string indexName, IndexConfiguration config, IndexFor indexFor)
        {
            CheckRoot();

            var baseEndpoint = GetUriForIndexType(indexFor);
            var createIndexApiRequest = new
            {
                name = indexName,
                config
            };

            return Request.With(ExecutionConfiguration)
                .Post(baseEndpoint)
                .WithJsonContent(SerializeAsJson(createIndexApiRequest))
                .WithExpectedStatusCodes(HttpStatusCode.Created)
                .ExecuteAsync();
        }

        public Task ReIndexAsync(NodeReference node, IEnumerable<IndexEntry> indexEntries)
        {
            var restPolicy = policyFactory.GetPolicy(PolicyType.Rest);
            var entityUri = restPolicy.BaseEndpoint.AddPath(node, restPolicy);
            var entityId = node.Id;
            return ReIndex(entityUri.ToString(), entityId, IndexFor.Node, indexEntries);
        }

        public Task ReIndexAsync(RelationshipReference relationship, IEnumerable<IndexEntry> indexEntries)
        {
            var restPolicy = policyFactory.GetPolicy(PolicyType.Rest);
            var entityUri = restPolicy.BaseEndpoint.AddPath(relationship, restPolicy);
            var entityId = relationship.Id;
            return ReIndex(entityUri.ToString(), entityId, IndexFor.Relationship, indexEntries);
        }

        private async Task ReIndex(string entityUri, long entityId, IndexFor indexFor, IEnumerable<IndexEntry> indexEntries,
            IExecutionPolicy policy)
        {
            if (indexEntries == null)
                throw new ArgumentNullException("indexEntries");

            CheckRoot();
            CheckTransactionEnvironmentWithPolicy(policy);

            var updates = indexEntries
                .SelectMany(
                    i => i.KeyValues,
                    (i, kv) => new { IndexName = i.Name, kv.Key, kv.Value })
                .Where(update => update.Value != null)
                .ToList();

            foreach (var indexName in updates.Select(u => u.IndexName).Distinct())
                await DeleteIndexEntries(indexName, entityId, GetUriForIndexType(indexFor)).ConfigureAwait(false);

            foreach (var update in updates)
                await AddIndexEntry(update.IndexName, update.Key, update.Value, entityUri, indexFor).ConfigureAwait(false);
        }

        public Task ReIndex(string entityUri, long entityId, IndexFor indexFor, IEnumerable<IndexEntry> indexEntries)
        {
            return ReIndex(entityUri, entityId, indexFor, indexEntries, GetPolicyForIndex(indexFor));
        }

        public Task DeleteIndexAsync(string indexName, IndexFor indexFor)
        {
            CheckRoot();
            var policy = GetPolicyForIndex(indexFor);
            CheckTransactionEnvironmentWithPolicy(policy);

            return Request.With(ExecutionConfiguration)
                .Delete(policy.BaseEndpoint.AddPath(indexName))
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .ExecuteAsync();
        }

        public Task DeleteIndexEntriesAsync(string indexName, NodeReference nodeReference)
        {
            return DeleteIndexEntries(indexName, nodeReference.Id, GetUriForIndexType(IndexFor.Node));
        }

        public Task DeleteIndexEntriesAsync(string indexName, RelationshipReference relationshipReference)
        {
            return DeleteIndexEntries(indexName, relationshipReference.Id, GetUriForIndexType(IndexFor.Relationship));
        }

        private Task DeleteIndexEntries(string indexName, long id, Uri indexUri)
        {
            var indexAddress = indexUri
                .AddPath(Uri.EscapeDataString(indexName))
                .AddPath(Uri.EscapeDataString(id.ToString(CultureInfo.InvariantCulture)));

            return Request.With(ExecutionConfiguration)
                .Delete(indexAddress)
                .WithExpectedStatusCodes(HttpStatusCode.NoContent)
                .ExecuteAsync(
                    string.Format("Deleting entries from index {0} for node {1}", indexName, id)
                );
        }

        private Task AddIndexEntry(string indexName, string indexKey, object indexValue, string address,
            IndexFor indexFor)
        {
            var encodedIndexValue = EncodeIndexValue(indexValue);
            if (string.IsNullOrWhiteSpace(encodedIndexValue))
#if NET45 
                return Task.FromResult(0);
#else
                return Task.CompletedTask;
#endif

            var indexAddress = BuildIndexAddress(indexName, indexFor);

            var indexEntry = new
            {
                key = indexKey,
                value = encodedIndexValue,
                uri = address
            };

            return Request.With(ExecutionConfiguration)
                .Post(indexAddress)
                .WithJsonContent(SerializeAsJson(indexEntry))
                .WithExpectedStatusCodes(HttpStatusCode.Created)
                .ExecuteAsync(string.Format("Adding '{0}'='{1}' to index {2} for {3}", indexKey, indexValue, indexName,
                    address));
        }

        private string BuildRelativeIndexAddress(string indexName, IndexFor indexFor)
        {
            var baseUri = indexFor == IndexFor.Node
                ? new UriBuilder() { Path = RootApiResponse.NodeIndex }
                : new UriBuilder() { Path = RootApiResponse.RelationshipIndex };
            return baseUri.Uri.AddPath(Uri.EscapeDataString(indexName)).LocalPath;
        }

        private Uri BuildIndexAddress(string indexName, IndexFor indexFor)
        {
            return GetUriForIndexType(indexFor).AddPath(Uri.EscapeDataString(indexName));
        }

        private static string EncodeIndexValue(object value)
        {
            string indexValue;
            if (value is DateTimeOffset)
            {
                indexValue = ((DateTimeOffset)value).UtcTicks.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is DateTime)
            {
                indexValue = ((DateTime)value).Ticks.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                indexValue = value.ToString();
            }

            if (string.IsNullOrWhiteSpace(indexValue) ||
                !indexValue.Any(char.IsLetterOrDigit))
                return string.Empty;

            return indexValue;
        }

        //ToDo Check status of https://github.com/neo4j/community/issues/249 for limiting query result sets
        [Obsolete(
            "There are encoding issues with this method. You should use the newer Cypher approach instead. See https://bitbucket.org/Readify/neo4jclient/issue/54/spaces-in-search-text-while-searching-for for an explanation of the problem, and https://bitbucket.org/Readify/neo4jclient/wiki/cypher for documentation about doing index queries with Cypher."
            )]
        public async Task<IEnumerable<Node<TNode>>> QueryIndexAsync<TNode>(string indexName, IndexFor indexFor, string query)
        {
            CheckRoot();
            var indexEndpoint = GetUriForIndexType(indexFor)
                .AddPath(indexName)
                .AddQuery("query=" + Uri.EscapeDataString(query));

            return (await Request.With(ExecutionConfiguration)
                .Get(indexEndpoint)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<NodeApiResponse<TNode>>>()
                .ExecuteAsync().ConfigureAwait(false))
                .Select(nodeResponse => nodeResponse.ToNode(this));
        }

        public Task<IEnumerable<Node<TNode>>> LookupIndexAsync<TNode>(string exactIndexName, IndexFor indexFor, string indexKey,
            long id)
        {
            return BuildLookupIndex<TNode>(exactIndexName, indexFor, indexKey, id.ToString(CultureInfo.InvariantCulture));
        }

        public Task<IEnumerable<Node<TNode>>> LookupIndexAsync<TNode>(string exactIndexName, IndexFor indexFor, string indexKey,
            int id)
        {
            return BuildLookupIndex<TNode>(exactIndexName, indexFor, indexKey, id.ToString(CultureInfo.InvariantCulture));
        }

        private async Task<IEnumerable<Node<TNode>>> BuildLookupIndex<TNode>(string exactIndexName, IndexFor indexFor,
            string indexKey, string id)
        {
            CheckRoot();
            var indexResource = GetUriForIndexType(indexFor)
                .AddPath(exactIndexName)
                .AddPath(indexKey)
                .AddPath(id.ToString());

            return (await Request.With(ExecutionConfiguration)
                .Get(indexResource)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<List<NodeApiResponse<TNode>>>()
                .ExecuteAsync().ConfigureAwait(false))
                .Select(query => query.ToNode(this));
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
