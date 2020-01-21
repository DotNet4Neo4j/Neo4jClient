using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;
using Neo4jClient.Transactions;
using Neo4jClient.Transactions.Bolt;
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
    public class BoltGraphClient : IBoltGraphClient, IRawGraphClient, ITransactionalGraphClient
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
        
        /// <summary>
        ///     Creates a new instance of the <see cref="BoltGraphClient" />.
        /// </summary>
        /// <param name="uri">
        ///     If the <paramref name="uris" /> parameter is provided, this will be treated as a <em>virtual URI</em>
        ///     , else it will be the URI connected to.
        /// </param>
        /// <param name="uris">
        ///     A collection of <see cref="Uri" /> instances to connect to using an
        ///     <see cref="IServerAddressResolver" />. Leave <c>null</c> (or empty) if you don't want to use it.
        /// </param>
        /// <param name="username">The username to connect to Neo4j with.</param>
        /// <param name="password">The password to connect to Neo4j with.</param>
        /// <param name="realm">The realm to connect to Neo4j with.</param>
        public BoltGraphClient(Uri uri, IEnumerable<Uri> uris, string username = null, string password = null, string realm = null)
        {
            var localUris = uris?.ToList();
            if (localUris != null && localUris.Any())
            {
                if (uri.Scheme.ToLowerInvariant() != "bolt+routing")
                    throw new NotSupportedException($"To use the {nameof(BoltGraphClient)} you need to provide a 'bolt://' scheme, not '{uri.Scheme}'.");

                addressResolver = new AddressResolver(uri, localUris);
            }
            else if (uri.Scheme.ToLowerInvariant() != "bolt" && uri.Scheme.ToLowerInvariant() != "bolt+routing")
            {
                throw new NotSupportedException($"To use the {nameof(BoltGraphClient)} you need to provide a 'bolt://' or 'bolt+routing://' scheme, not '{uri.Scheme}'.");
            }

            this.uri = uri;
            this.username = username;
            this.password = password;
            this.realm = realm;
            PolicyFactory = new ExecutionPolicyFactory(this);

            JsonConverters = new List<JsonConverter>();
            JsonConverters.AddRange(DefaultJsonConverters);
            JsonContractResolver = DefaultJsonContractResolver;

            ExecutionConfiguration = new ExecutionConfiguration
            {
                UserAgent = $"Neo4jClient/{GetType().GetTypeInfo().Assembly.GetName().Version}",
                UseJsonStreaming = true,
                JsonConverters = JsonConverters,
                Username = username,
                Password = password,
                Realm = realm
            };

            transactionManager = new BoltTransactionManager(this);
        }

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
            : this(new Uri("neo4j://Neo4j-Driver-Does-Not-Supply-This/"), null, null, null)
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

            public void Complete(CypherQuery query, Bookmark lastBookmark)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, 0, null, identifier: query.Identifier, bookmarks: query.Bookmarks);
            }

            public void Complete(CypherQuery query, Bookmark lastBookmark, int resultsCount)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, resultsCount, null, query.CustomHeaders, identifier: query.Identifier, bookmarks: query.Bookmarks);
            }

            public void Complete(CypherQuery query, Bookmark lastBookmark, Exception exception)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, -1, exception, identifier:query.Identifier, bookmarks:query.Bookmarks);
            }

            public void Complete(string queryText, Bookmark lastBookmark, int resultsCount = -1, Exception exception = null, NameValueCollection customHeaders = null, int? maxExecutionTime = null, string identifier = null, IEnumerable<string> bookmarks = null)
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
        public Task<NodeReference<TNode>> CreateAsync<TNode>(TNode node, IEnumerable<IRelationshipAllowingParticipantNode<TNode>> relationships, IEnumerable<IndexEntry> indexEntries)
            where TNode : class
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
        public Task<Node<TNode>> GetAsync<TNode>(NodeReference<TNode> reference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<RelationshipInstance<TData>> GetAsync<TData>(RelationshipReference<TData> reference) where TData : class, new()
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
        public Task UpdateAsync<TNode>(NodeReference<TNode> nodeReference, TNode replacementData, IEnumerable<IndexEntry> indexEntries = null)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<Node<TNode>> UpdateAsync<TNode>(NodeReference<TNode> nodeReference, Action<TNode> updateCallback, Func<TNode, IEnumerable<IndexEntry>> indexEntriesCallback = null, Action<IEnumerable<FieldChange>> changeCallback = null)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task UpdateAsync<TRelationshipData>(RelationshipReference<TRelationshipData> relationshipReference, Action<TRelationshipData> updateCallback) where TRelationshipData : class, new()
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task DeleteAsync(NodeReference reference, DeleteMode mode)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<RelationshipReference> CreateRelationshipAsync<TSourceNode, TRelationship>(NodeReference<TSourceNode> sourceNodeReference, TRelationship relationship) where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSourceNode>
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task DeleteRelationshipAsync(RelationshipReference reference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<Dictionary<string, IndexMetaData>> GetIndexesAsync(IndexFor indexFor)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<bool> CheckIndexExistsAsync(string indexName, IndexFor indexFor)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task CreateIndexAsync(string indexName, IndexConfiguration config, IndexFor indexFor)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task ReIndexAsync(NodeReference node, IEnumerable<IndexEntry> indexEntries)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task ReIndexAsync(RelationshipReference relationship, IEnumerable<IndexEntry> indexEntries)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task DeleteIndexAsync(string indexName, IndexFor indexFor)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task DeleteIndexEntriesAsync(string indexName, NodeReference relationshipReference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task DeleteIndexEntriesAsync(string indexName, RelationshipReference relationshipReference)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<IEnumerable<Node<TNode>>> QueryIndexAsync<TNode>(string indexName, IndexFor indexFor, string query)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<IEnumerable<Node<TNode>>> LookupIndexAsync<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, long id)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

        /// <inheritdoc />
        [Obsolete(NotValidForBolt)]
        public Task<IEnumerable<Node<TNode>>> LookupIndexAsync<TNode>(string exactIndexName, IndexFor indexFor, string indexKey, int id)
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
        public async Task ConnectAsync(NeoServerConfiguration configuration = null)
        {
            if (Driver == null)
            {
                var driver = configuration == null
                    ? new DriverWrapper(uri, addressResolver, username, password, realm)
                    : new DriverWrapper(uri, addressResolver, configuration.Username, configuration.Password, configuration.Realm);
                Driver = driver;
            }

            var session = Driver.AsyncSession(x => x.WithDefaultAccessMode(AccessMode.Read));

            var serverInformation = await session.RunAsync("CALL dbms.components()").ConfigureAwait(false);
            foreach (var record in await serverInformation.ToListAsync().ConfigureAwait(false))
            {
                var name = record["name"].As<string>();
                if (name.ToLowerInvariant() != "neo4j kernel")
                    continue;

                var version = record["versions"].As<List<object>>();
                ServerVersion = RootApiResponse.GetVersion(version?.First()?.ToString());

                if (ServerVersion > new Version(3, 0))
                    CypherCapabilities = CypherCapabilities.Cypher30;
            }

            await session.CloseAsync();

            IsConnected = true;
        }

        /// <inheritdoc />
        public List<JsonConverter> JsonConverters { get; }

        /// <inheritdoc />
        public DefaultContractResolver JsonContractResolver { get; set; }

#endregion

        #region Implementation of IRawGraphClient

        /// <inheritdoc />
        async Task<IEnumerable<TResult>> IRawGraphClient.ExecuteGetCypherResultsAsync<TResult>(CypherQuery query)
        {
            if (Driver == null)
                throw new InvalidOperationException("Can't execute cypher unless you have connected to the server.");

            var context = ExecutionContext.Begin(this);
            List<TResult> results;
            Bookmark lastBookmark = null;
            try
            {
                if (InTransaction)
                {
                    var result = await transactionManager.EnqueueCypherRequest($"The query was: {query.QueryText}", this, query).ConfigureAwait(false);
                    results = ParseResults<TResult>(await result.StatementResult.ToListAsync().ConfigureAwait(false), query);
                }
                else
                {
                    var session = Driver.AsyncSession(x => x.WithDefaultAccessMode(query.IsWrite ? AccessMode.Write : AccessMode.Read).WithBookmarks(Bookmark.From(query.Bookmarks.ToArray())));

                    var result = query.IsWrite
                        ? await session.WriteTransactionAsync(s => s.RunAsync(query, this)).ConfigureAwait(false)
                        : await session.ReadTransactionAsync(s => s.RunAsync(query, this)).ConfigureAwait(false);

                    results = ParseResults<TResult>(await result.ToListAsync().ConfigureAwait(false), query);
                    lastBookmark = session.LastBookmark;
                    await session.CloseAsync();
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

        private List<TResult> ParseResults<TResult>(IEnumerable<IRecord> result, CypherQuery query)
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
       async Task IRawGraphClient.ExecuteCypherAsync(CypherQuery query)
        {
           var tx = ExecutionContext.Begin(this);

            if (Driver == null)
                throw new InvalidOperationException("Can't execute cypher unless you have connected to the server.");

            if (InTransaction)
            {
                await transactionManager.EnqueueCypherRequest($"The query was: {query.QueryText}", this, query).ConfigureAwait(false);
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs
                {
                    QueryText = $"BOLT:{query.QueryText}"
                });
            }

            var session = Driver.AsyncSession(x => x.WithDefaultAccessMode(query.IsWrite ? AccessMode.Write : AccessMode.Read).WithBookmarks().WithBookmarks(Bookmark.From(query.Bookmarks.ToArray())) );
            {
                if (query.IsWrite)
                    await session.WriteTransactionAsync(async s => await s.RunAsync(query, this)).ConfigureAwait(false);
                else
                    await session.ReadTransactionAsync(async s => await s.RunAsync(query, this)).ConfigureAwait(false);
                tx.Complete(query, session.LastBookmark);
            }
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