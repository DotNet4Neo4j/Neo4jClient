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
            new EnumValueConverter(),
            new ZonedDateTimeConverter(), 
            new LocalDateTimeConverter()
        };

        private static readonly DefaultContractResolver DefaultJsonContractResolver = new DefaultContractResolver();

        private readonly string password;
        private readonly string realm;

        private readonly ITransactionManager<BoltResponse> transactionManager;
        private readonly IServerAddressResolver addressResolver;
        private readonly string username;
        private readonly Uri uri;

        public BoltGraphClient(Uri uri, string username = null, string password = null, string realm = null)
            : this(uri, null, username, password, realm)
        { }

        public BoltGraphClient(IEnumerable<string> uris, string username = null, string password = null, string realm = null)
            : this(new Uri("bolt+routing://virtual.neo4j.uri"), uris?.Select(UriCreator.From).ToList(), username, password, realm)
        { }

        public BoltGraphClient(string uri, IEnumerable<string> uris, string username = null, string password = null, string realm = null)
        : this(new Uri(uri), uris?.Select(UriCreator.From).ToList(), username, password, realm)
        {}

        public BoltGraphClient(string uri, string username = null, string password= null, string realm = null)
            : this(new Uri(uri), username, password, realm)
        { }

        public BoltGraphClient(IDriver driver)
            : this(driver.Uri, null, null, null)
        {
            Driver = driver;
        }

        internal IDriver Driver { get; set; }
        internal IServerAddressResolver AddressResolver => addressResolver;
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

            public void Complete(CypherQuery query, string lastBookmark)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, 0, null, identifier: query.Identifier, bookmarks: query.Bookmarks);
            }

            public void Complete(CypherQuery query, string lastBookmark, int resultsCount)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, resultsCount, null, query.CustomHeaders, identifier: query.Identifier, bookmarks: query.Bookmarks);
            }

            public void Complete(CypherQuery query, string lastBookmark, Exception exception)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, -1, exception, identifier:query.Identifier, bookmarks:query.Bookmarks);
            }

            public void Complete(string queryText, string lastBookmark, int resultsCount = -1, Exception exception = null, NameValueCollection customHeaders = null, int? maxExecutionTime = null, string identifier = null, IEnumerable<string> bookmarks = null)
            {
                var args = new OperationCompletedEventArgs
                {
                    LastBookmark = lastBookmark,
                    QueryText = queryText,
                    ResourcesReturned = resultsCount,
                    TimeTaken = stopwatch.Elapsed,
                    Exception = exception,
                    CustomHeaders = customHeaders,
                    MaxExecutionTime = maxExecutionTime,
                    Identifier = identifier,
                    BookmarksUsed = bookmarks
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
                    ? new DriverWrapper(uri, addressResolver, username, password, realm)
                    : new DriverWrapper(uri, addressResolver, configuration.Username, configuration.Password, configuration.Realm);
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
            string lastBookmark = null;
            try
            {
                if (InTransaction)
                {
                    var result = await transactionManager.EnqueueCypherRequest($"The query was: {query.QueryText}", this, query).ConfigureAwait(false);
                    results = ParseResults<TResult>(result.StatementResult, query);
                }
                else
                {
                    using (var session = Driver.Session(query.IsWrite ? AccessMode.Write : AccessMode.Read, query.Bookmarks))
                    {
                        var result = query.IsWrite 
                            ? session.WriteTransaction(s => s.Run(query, this)) 
                            : session.ReadTransaction(s => s.Run(query, this));

                        results = ParseResults<TResult>(result, query);
                        lastBookmark = session.LastBookmark;
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                Exception unwrappedException;
                context.Complete(query, lastBookmark, aggregateException.TryUnwrap(out unwrappedException) ? unwrappedException : aggregateException);
                throw;
            }
            catch (Exception e)
            {
                context.Complete(query, lastBookmark, e);
                throw;
            }

            context.Complete(query, lastBookmark, results.Count);
            return results;
        }

        private List<TResult> ParseResults<TResult>(IStatementResult result, CypherQuery query)
        {
            var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode, query.ResultFormat, false, true);
            var results = new List<TResult>();
            if (typeof(TResult).IsAnonymous())
            {
                foreach (var record in result)
                    results.AddRange(deserializer.Deserialize(record.ParseAnonymous(this)));
            }
            else
            {

                StatementResultHelper.JsonSettings = new JsonSerializerSettings
                {
                    Converters = JsonConverters,
                    ContractResolver = JsonContractResolver
                };

                List<IEnumerable<TResult>> converted = new List<IEnumerable<TResult>>();
                foreach (var record in result)
                {
                    var des = record.Deserialize(deserializer, query.ResultMode);
                    converted.Add(des);
                }

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
        [Obsolete]
        void IRawGraphClient.ExecuteMultipleCypherQueriesInTransaction(IEnumerable<CypherQuery> queries, NameValueCollection customHeaders = null)
        {
            using (var tx = BeginTransaction())
            {
                // the HTTP endpoint executed the transactions in a serial fashion
                foreach (var query in queries)
                {
                    ((IRawGraphClient) this).ExecuteCypher(query);
                }

                tx.Commit();
            }
        }

        /// <inheritdoc />
       Task IRawGraphClient.ExecuteCypherAsync(CypherQuery query)
        {
           var tx = ExecutionContext.Begin(this);

            if (Driver == null)
                throw new InvalidOperationException("Can't execute cypher unless you have connected to the server.");

            if (InTransaction)
                return transactionManager.EnqueueCypherRequest($"The query was: {query.QueryText}", this, query)
                    .ContinueWith(responseTask => OperationCompleted?.Invoke(this, new OperationCompletedEventArgs
                    {
                        QueryText = $"BOLT:{query.QueryText}"
                    }));

            using (var session = Driver.Session(query.IsWrite ? AccessMode.Write : AccessMode.Read, query.Bookmarks))
            {
                if (query.IsWrite)
                    session.WriteTransaction(s => s.Run(query, this));
                else
                    session.ReadTransaction(s => s.Run(query, this));
                tx.Complete(query, session.LastBookmark);
            }
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
            return BeginTransaction((IEnumerable<string>) null);
        }
        
        /// <inheritdoc />
        public ITransaction BeginTransaction(string bookmark)
        {
            return BeginTransaction(new List<string>{bookmark});
        }
        
        /// <inheritdoc />
        public ITransaction BeginTransaction(IEnumerable<string> bookmarks)
        {
            return BeginTransaction(TransactionScopeOption.Join, bookmarks);
        }

        /// <inheritdoc />
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption)
        {
            return BeginTransaction(scopeOption, (IEnumerable<string>) null);
        }

        /// <inheritdoc />
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, string bookmark)
        {
            return BeginTransaction(scopeOption, new List<string>{bookmark});
        }

        /// <inheritdoc />
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, IEnumerable<string> bookmarks)
        {
            if (transactionManager == null)
                throw new NotSupportedException("HTTP Transactions are only supported on Neo4j 2.0 and newer.");

            return transactionManager.BeginTransaction(scopeOption, bookmarks);
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
        public Uri RootEndpoint => throw new InvalidOperationException(NotValidForBolt);

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Uri BatchEndpoint => throw new InvalidOperationException(NotValidForBolt);

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Uri CypherEndpoint => throw new InvalidOperationException(NotValidForBolt);

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public ISerializer Serializer => throw new InvalidOperationException(NotValidForBolt);

        /// <inheritdoc />
        public ExecutionConfiguration ExecutionConfiguration { get; }

        /// <inheritdoc />
        public bool IsConnected { get; private set; }

        /// <summary>Raises the <see cref="OperationCompleted"/> event.</summary>
        /// <param name="args">The instance of <see cref="OperationCompletedEventArgs"/> to send to listeners.</param>
        protected void OnOperationCompleted(OperationCompletedEventArgs args)
        {
            OperationCompleted?.Invoke(this, args);
        }

        #endregion

        
    }
}