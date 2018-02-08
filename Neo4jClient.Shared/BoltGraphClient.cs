using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Gremlin;
using Neo4jClient.Serialization;
using Neo4jClient.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ITransaction = Neo4jClient.Transactions.ITransaction;

//TODO: Logging
//TODO: Config Stuff
//TODO: Transaction Stuff

namespace Neo4jClient
{
    internal class DriverWrapper : IDriver
    {
        private readonly IDriver driver;
        public string Username { get;  }
        public string Password { get; }
        public string Realm { get; }

        public DriverWrapper(IDriver driver)
        {
            this.driver = driver;
        }

        public DriverWrapper(string uri, string username, string pass, string realm)
            :this(new Uri(uri), username, pass, realm)
        {
        }
        public DriverWrapper(Uri uri, string username, string pass, string realm)
        {
            Uri = uri;
            Username = username;
            Password = pass;
            Realm = realm;

            var authToken = GetAuthToken(username, pass, realm);
            this.driver = GraphDatabase.Driver(uri, authToken);
        }


        public ISession Session()
        {
            return driver.Session();
        }

        public ISession Session(AccessMode defaultMode)
        {
            return driver.Session(defaultMode);
        }

        public ISession Session(string bookmark)
        {
            return driver.Session(bookmark);
        }

        public ISession Session(AccessMode defaultMode, string bookmark)
        {
            return driver.Session(defaultMode, bookmark);
        }

        public ISession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return driver.Session(defaultMode, bookmarks);
        }

        public ISession Session(IEnumerable<string> bookmarks)
        {
            return driver.Session(bookmarks);
        }

        public void Close()
        {
            driver.Close();
        }

        public Task CloseAsync()
        {
            return driver.CloseAsync();
        }

        public Uri Uri { get; }

        private static IAuthToken GetAuthToken(string username, string password, string realm)
        {
            return string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
                ? AuthTokens.None
                : AuthTokens.Basic(username, password, realm);
        }
        public void Dispose()
        {
            driver?.Dispose();
        }
    }

    /// <summary>
    ///     The <see cref="BoltGraphClient" /> is the client for connecting to the Bolt protocol of Neo4j.
    /// </summary>
    public partial class BoltGraphClient : IBoltGraphClient, IRawGraphClient, ITransactionalGraphClient
    {
        internal const string NotValidForBolt = "This is not available using the BoltGraphClient.";

        internal static readonly JsonConverter[] DefaultJsonConverters =
        {
            new TypeConverterBasedJsonConverter(),
            new NullableEnumValueConverter(),
            new TimeZoneInfoConverter(),
            new EnumValueConverter()
        };

        private static readonly DefaultContractResolver DefaultJsonContractResolver = new DefaultContractResolver();

        private readonly string password;
        private readonly string realm;

        private readonly ITransactionManager<BoltResponse> transactionManager;
        private readonly Uri uri;
        private readonly string username;

        public BoltGraphClient(string uri, string username = null, string password= null, string realm = null)
            : this(new Uri(uri), username, password, realm)
        { }

        public BoltGraphClient(IDriver driver)
            : this(driver.Uri, null, null, null)
        {
            Driver = driver;
        }

        internal IDriver Driver { get; set; }
        private IExecutionPolicyFactory PolicyFactory { get; }

        #region Implementation of ICypherGraphClient

        /// <inheritdoc />
        public ICypherFluentQuery Cypher => new CypherFluentQuery(this, true);
        #endregion
        
        private void CheckTransactionEnvironmentWithPolicy(IExecutionPolicy policy)
        {
            var inTransaction = InTransaction;

            transactionManager?.RegisterToTransactionIfNeeded();

            if (inTransaction && policy.TransactionExecutionPolicy == TransactionExecutionPolicy.Denied)
                throw new InvalidOperationException("Cannot be done inside a transaction scope.");

            if (!inTransaction && policy.TransactionExecutionPolicy == TransactionExecutionPolicy.Required)
                throw new InvalidOperationException("Cannot be done outside a transaction scope.");
        }

        #region ExecutionContext class

        internal class ExecutionContext
        {
            private readonly Stopwatch stopwatch;
            private BoltGraphClient owner;

            private ExecutionContext()
            {
                stopwatch = Stopwatch.StartNew();
            }

            public IExecutionPolicy Policy { get; set; }
            public static bool HasErrors { get; set; }

            public static ExecutionContext Begin(BoltGraphClient owner)
            {
                var policy = owner.PolicyFactory.GetPolicy(PolicyType.Cypher);

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

        #region Implementation of IDisposable

        /// <inheritdoc />
        protected void Dispose(bool isDisposing)
        {
            if (!isDisposing)
                return;

            Driver?.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Implementation of IGraphClient

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public RootNode RootNode
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public NodeReference<TNode> Create<TNode>(TNode node, IEnumerable<IRelationshipAllowingParticipantNode<TNode>> relationships, IEnumerable<IndexEntry> indexEntries)
            where TNode : class
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Node<TNode> Get<TNode>(NodeReference reference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<Node<TNode>> GetAsync<TNode>(NodeReference reference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Node<TNode> Get<TNode>(NodeReference<TNode> reference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public RelationshipInstance<TData> Get<TData>(RelationshipReference<TData> reference) 
            where TData : class, new()
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public RelationshipInstance<TData> Get<TData>(RelationshipReference reference) where TData : class, new()
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<RelationshipInstance<TData>> GetAsync<TData>(RelationshipReference reference) where TData : class, new()
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void Update<TNode>(NodeReference<TNode> nodeReference, TNode replacementData, IEnumerable<IndexEntry> indexEntries = null)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Node<TNode> Update<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback, Func<TNode, IEnumerable<IndexEntry>> indexEntriesCallback = null, Action<IEnumerable<FieldChange>> changeCallback = null)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void Update<TRelationshipData>(RelationshipReference<TRelationshipData> relationshipReference, Action<TRelationshipData> updateCallback) where TRelationshipData : class, new()
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void Delete(NodeReference reference, DeleteMode mode)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public RelationshipReference CreateRelationship<TSourceNode, TRelationship>(NodeReference<TSourceNode> sourceNodeReference, TRelationship relationship) where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSourceNode>
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void DeleteRelationship(RelationshipReference reference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public string ExecuteScalarGremlin(string query, IDictionary<string, object> parameters)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public IEnumerable<TResult> ExecuteGetAllProjectionsGremlin<TResult>(IGremlinQuery query) where TResult : new()
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public IEnumerable<Node<TNode>> ExecuteGetAllNodesGremlin<TNode>(IGremlinQuery query)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public IEnumerable<RelationshipInstance> ExecuteGetAllRelationshipsGremlin(string query, IDictionary<string, object> parameters)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public IEnumerable<RelationshipInstance<TData>> ExecuteGetAllRelationshipsGremlin<TData>(string query, IDictionary<string, object> parameters) where TData : class, new()
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Dictionary<string, IndexMetaData> GetIndexes(IndexFor indexFor)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public bool CheckIndexExists(string indexName, IndexFor indexFor)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void CreateIndex(string indexName, IndexConfiguration config, IndexFor indexFor)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void ReIndex(NodeReference node, IEnumerable<IndexEntry> indexEntries)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void ReIndex(RelationshipReference relationship, IEnumerable<IndexEntry> indexEntries)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void DeleteIndex(string indexName, IndexFor indexFor)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void DeleteIndexEntries(string indexName, NodeReference relationshipReference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void DeleteIndexEntries(string indexName, RelationshipReference relationshipReference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public IEnumerable<Node<TNode>> QueryIndex<TNode>(string indexName, IndexFor indexFor, string query)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public IEnumerable<Node<TNode>> LookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, long id)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public IEnumerable<Node<TNode>> LookupIndex<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, int id)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public void ShutdownServer()
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public IGremlinClient Gremlin
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Uri GremlinEndpoint
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Uri NodeIndexEndpoint
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }


        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Uri RelationshipIndexEndpoint
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

        /// <inheritdoc />
        public void Connect(NeoServerConfiguration configuration = null)
        {
            var connectTask = ConnectAsync(configuration);
            connectTask.Wait();
        }

        /// <inheritdoc />
        public Task ConnectAsync(NeoServerConfiguration configuration = null)
        {
            if (Driver == null)
            {
                var driver = configuration == null
                    ? new DriverWrapper(uri, username, password, realm)
                    : new DriverWrapper(uri, configuration.Username, configuration.Password, configuration.Realm);
                Driver = driver;
            }

            using (var session = Driver.Session(AccessMode.Read))
            {
                var serverInformation = session.Run("CALL dbms.components()");
                foreach (var record in serverInformation)
                {
                    var name = record["name"].As<string>();
                    if (name.ToLowerInvariant() != "neo4j kernel")
                        continue;

                    var version = record["versions"].As<List<object>>();
                    ServerVersion = RootApiResponse.GetVersion(version?.First()?.ToString());

                    if (ServerVersion > new Version(3, 0))
                        CypherCapabilities = CypherCapabilities.Cypher30;
                }
            }

            IsConnected = true;
#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        /// <inheritdoc />
        public List<JsonConverter> JsonConverters { get; }

        /// <inheritdoc />
        public DefaultContractResolver JsonContractResolver { get; set; }

#endregion

#region Implementation of IRawGraphClient

        /// <inheritdoc />
        IEnumerable<TResult> IRawGraphClient.ExecuteGetCypherResults<TResult>(CypherQuery query)
        {
            var task = ((IRawGraphClient) this).ExecuteGetCypherResultsAsync<TResult>(query);
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException ex)
            {
                Exception unwrappedException;
                if (ex.TryUnwrap(out unwrappedException))
                    throw unwrappedException;
                throw;
            }

            return task.Result;
        }

        /// <inheritdoc />
        async Task<IEnumerable<TResult>> IRawGraphClient.ExecuteGetCypherResultsAsync<TResult>(CypherQuery query)
        {
            if (Driver == null)
                throw new InvalidOperationException("Can't execute cypher unless you have connected to the server.");

            var context = ExecutionContext.Begin(this);
            List<TResult> results;
            try
            {
//                var inTransaction = ;
                if (InTransaction)
                {
                    var result = await transactionManager.EnqueueCypherRequest($"The query was: {query.QueryText}", this, query).ConfigureAwait(false);
                    results = ParseResults<TResult>(result.StatementResult, query, true);
                }
                else
                {

                    using (var session = Driver.Session(query.IsWrite ? AccessMode.Write : AccessMode.Read))
                    {
                        
                        var result = session.Run(query, this);
                        results = ParseResults<TResult>(result, query, false);
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                Exception unwrappedException;
                context.Complete(query, aggregateException.TryUnwrap(out unwrappedException) ? unwrappedException : aggregateException);
                throw;
            }
            catch (Exception e)
            {
                context.Complete(query, e);
                throw;
            }

            context.Complete(query, results.Count); //Doesn't this parse all the entries?
            return results;
        }

        private List<TResult> ParseResults<TResult>(IStatementResult result, CypherQuery query, bool inTransaction)
        {
            var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode, query.ResultFormat, inTransaction);
            var results = new List<TResult>();
            if (typeof(TResult).IsAnonymous())
            {
                foreach (var record in result)
                    results.AddRange(deserializer.Deserialize(record.ParseAnonymous(this)));
            }
            else
            {
                var converted = result.Select(record => record.Deserialize(deserializer, query.ResultMode));
                foreach (var enumerable in converted)
                {
                    results.AddRange(enumerable);
                }
            }

            return results;
        }

        /// <inheritdoc />
        void IRawGraphClient.ExecuteCypher(CypherQuery query)
        {
            var task = ((IRawGraphClient) this).ExecuteCypherAsync(query);
            task.Wait();
        }

        /// <inheritdoc />
        
        void IRawGraphClient.ExecuteMultipleCypherQueriesInTransaction(IEnumerable<CypherQuery> queries, NameValueCollection customHeaders = null)
        {
//            var context = ExecutionContext.Begin(this);
//
//            var queryList = queries.ToList();
//            string queriesInText = string.Join(", ", queryList.Select(query => query.QueryText));
//
//            var stopwatch = new Stopwatch();
//            stopwatch.Start();
//
//            var response = Request.With(ExecutionConfiguration, customHeaders)
//                .Post(context.Policy.BaseEndpoint)
//                .WithJsonContent(SerializeAsJson(new CypherStatementList(queryList)))
//                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.Created)
//                .Execute("Executing multiple queries: " + queriesInText);
//
//            var transactionObject = transactionManager.CurrentNonDtcTransaction ??
//                                    transactionManager.CurrentDtcTransaction;
//
//            if (customHeaders != null && customHeaders.Count > 0)
//            {
//                transactionObject.CustomHeaders = customHeaders;
//            }
//
//            context.Policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(response), transactionObject);
//            context.Complete(OperationCompleted != null ? string.Join(", ", queryList.Select(query => query.DebugQueryText)) : string.Empty);
//            context.Policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(response), transactionObject);

            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
       Task IRawGraphClient.ExecuteCypherAsync(CypherQuery query)
        {
           var tx=  ExecutionContext.Begin(this);

            if (Driver == null)
                throw new InvalidOperationException("Can't execute cypher unless you have connected to the server.");

            if (InTransaction)
                return transactionManager.EnqueueCypherRequest($"The query was: {query.QueryText}", this, query)
                    .ContinueWith(responseTask => OperationCompleted?.Invoke(this, new OperationCompletedEventArgs
                    {
                        QueryText = $"BOLT:{query.QueryText}"
                    }));

            using (var session = Driver.Session(query.IsWrite ? AccessMode.Write : AccessMode.Read))
            {
                session.Run(query, this);
            }

            tx.Complete(query);
#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        #endregion

        #region Implementation of ITransactionalGraphClient

        /// <inheritdoc />
        public ITransaction BeginTransaction()
        {
            return BeginTransaction(TransactionScopeOption.Join);
        }

        /// <inheritdoc />
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption)
        {
            if (transactionManager == null)
                throw new NotSupportedException("HTTP Transactions are only supported on Neo4j 2.0 and newer.");

            return transactionManager.BeginTransaction(scopeOption);
        }

        /// <inheritdoc />
        public ITransaction Transaction => transactionManager?.CurrentNonDtcTransaction;

        /// <inheritdoc />
        public bool InTransaction => transactionManager != null && transactionManager.InTransaction;

        /// <inheritdoc />
        public void EndTransaction()
        {
            if (transactionManager == null)
                throw new NotSupportedException("Transactions are only supported on Neo4j 2.0 and newer.");

            transactionManager.EndTransaction();
        }

        /// <inheritdoc />
        public Uri TransactionEndpoint
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

#endregion

#region Implementation of IBoltGraphClient

        /// <inheritdoc />
        public event OperationCompletedEventHandler OperationCompleted;

        /// <inheritdoc />
        public CypherCapabilities CypherCapabilities { get; private set; }

        /// <inheritdoc />
        public Version ServerVersion { get; private set; }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Uri RootEndpoint
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Uri BatchEndpoint
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Uri CypherEndpoint
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public ISerializer Serializer
        {
            get { throw new InvalidOperationException(NotValidForBolt); }
        }

        /// <inheritdoc />
        public ExecutionConfiguration ExecutionConfiguration { get; }

        /// <inheritdoc />
        public bool IsConnected { get; private set; }

        protected void OnOperationCompleted(OperationCompletedEventArgs args)
        {
            OperationCompleted?.Invoke(this, args);
        }

#endregion
    }
}