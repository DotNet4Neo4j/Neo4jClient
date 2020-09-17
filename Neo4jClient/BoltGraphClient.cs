using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Cypher;
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
    using Neo4jClient.Extensions;

    /// <summary>
    ///     The <see cref="BoltGraphClient" /> is the client for connecting to the Bolt protocol of Neo4j.
    /// </summary>
    public class BoltGraphClient : IBoltGraphClient, IRawGraphClient, ITransactionalGraphClient
    {
        public ITransactionalGraphClient Tx => this;

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
        private readonly EncryptionLevel? encryptionLevel;

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
        public BoltGraphClient(Uri uri, IEnumerable<Uri> uris, string username = null, string password = null, string realm = null, EncryptionLevel? encryptionLevel = null)
        {
            var localUris = uris?.ToList();
            if (localUris != null && localUris.Any())
            {
                //TODO - const/etc these
                if (!new [] {"neo4j", "neo4j+s", "neo4j+ssc"}.Contains(uri.Scheme.ToLowerInvariant()))
                    throw new NotSupportedException($"To use the {nameof(BoltGraphClient)} you need to provide a 'bolt://' scheme, not '{uri.Scheme}'.");

                addressResolver = new AddressResolver(uri, localUris);
            }
            else if (!new [] {"neo4j", "neo4j+s", "neo4j+ssc", "bolt", "bolt+s", "bolt+ssc"}.Contains(uri.Scheme.ToLowerInvariant()))
            {
                throw new NotSupportedException($"To use the {nameof(BoltGraphClient)} you need to provide a 'bolt://' or 'neo4j://' scheme, not '{uri.Scheme}'.");
            }

            this.uri = uri;
            this.username = username;
            this.password = password;
            this.realm = realm;
            this.encryptionLevel = encryptionLevel;
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

        public BoltGraphClient(Uri uri, string username = null, string password = null, string realm = null, EncryptionLevel? encryptionLevel = null)
            : this(uri, null, username, password, realm, encryptionLevel)
        { }

        public BoltGraphClient(IEnumerable<string> uris, string username = null, string password = null, string realm = null, EncryptionLevel? encryptionLevel = null)
            : this(new Uri("neo4j://virtual.neo4j.uri"), uris?.Select(UriCreator.From).ToList(), username, password, realm, encryptionLevel)
        { }

        public BoltGraphClient(string uri, IEnumerable<string> uris, string username = null, string password = null, string realm = null, EncryptionLevel? encryptionLevel = null)
        : this(new Uri(uri), uris?.Select(UriCreator.From).ToList(), username, password, realm, encryptionLevel)
        {}

        public BoltGraphClient(string uri, string username = null, string password= null, string realm = null, EncryptionLevel? encryptionLevel = null)
            : this(new Uri(uri), username, password, realm, encryptionLevel)
        { }

        public BoltGraphClient(IDriver driver)
            : this(new Uri("neo4j://Neo4j-Driver-Does-Not-Supply-This/"), null, null, null, null)
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
            public string Database { get; set; }
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

            public void Complete(CypherQuery query, Bookmark lastBookmark, QueryStats queryStats)
            {
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, 0, null, identifier: query.Identifier, bookmarks: query.Bookmarks, stats:queryStats);
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

            public void Complete(CypherQuery query, Bookmark lastBookmark, int resultsCount, QueryStats stats)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, resultsCount, null, query.CustomHeaders, identifier: query.Identifier, bookmarks: query.Bookmarks, stats: stats);
            }

            public void Complete(CypherQuery query, Bookmark lastBookmark, Exception exception)
            {
                // only parse the events when there's an event handler
                Complete(owner.OperationCompleted != null ? query.DebugQueryText : string.Empty, lastBookmark, -1, exception, identifier:query.Identifier, bookmarks:query.Bookmarks);
            }

            public void Complete(string queryText, Bookmark lastBookmark, int resultsCount = -1, Exception exception = null, NameValueCollection customHeaders = null, int? maxExecutionTime = null, string identifier = null, IEnumerable<Bookmark> bookmarks = null, QueryStats stats = null)
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
                    BookmarksUsed = bookmarks,
                    QueryStats = stats
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


        /// <inheritdoc cref="IGraphClient.ConnectAsync"/>
        public async Task ConnectAsync(NeoServerConfiguration configuration = null)
        {
            if (Driver == null)
            {
                var driver = configuration == null
                    ? new DriverWrapper(uri, addressResolver, username, password, realm, encryptionLevel)
                    : new DriverWrapper(uri, addressResolver, configuration.Username, configuration.Password, configuration.Realm, configuration.EncryptionLevel);
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
                if(ServerVersion >= new Version(4,0))
                    CypherCapabilities = CypherCapabilities.Cypher40;
            }

            await session.CloseAsync();

            IsConnected = true;
        }

        /// <inheritdoc />
        public List<JsonConverter> JsonConverters { get; }

        /// <inheritdoc />
        public DefaultContractResolver JsonContractResolver { get; set; }

        public Uri GetTransactionEndpoint(string database, bool autoCommit = false)
        {
            throw new InvalidOperationException(NotValidForBolt);
        }

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
            QueryStats stats = null;

            async Task<QueryStats> GetQueryStats(IResultCursor resultCursor)
            {
                if (!query.IncludeQueryStats) return null;
                var summary = await resultCursor.ConsumeAsync();
                stats = new QueryStats(summary.Counters);

                return stats;
            }

            try
            {
                if (InTransaction)
                {
                    context.Database = Transaction.Database;
                    var result = await transactionManager.EnqueueCypherRequest($"The query was: {query.QueryText}", this, query).ConfigureAwait(false);
                    results = ParseResults<TResult>(await result.StatementResult.ToListAsync().ConfigureAwait(false), query);
                    if (query.IncludeQueryStats)
                    {
                        var summary = await result.StatementResult.ConsumeAsync();
                        stats = new QueryStats(summary.Counters);
                    }
                }
                else
                {
                    var session = Driver.AsyncSession(ServerVersion, query.Database, query.IsWrite, query.Bookmarks);

                    async Task<List<IRecord>> Records(IAsyncTransaction asyncTransaction)
                    {
                        var cursor = await asyncTransaction.RunAsync(query, this).ConfigureAwait(false);
                        var output = await cursor.ToListAsync().ConfigureAwait(false);
                        stats = await GetQueryStats(cursor);
                        return output;
                    }

                    var result = query.IsWrite 
                        ? await session.WriteTransactionAsync(async s => await Records(s)).ConfigureAwait(false)
                        : await session.ReadTransactionAsync(async s => await Records(s)).ConfigureAwait(false);

                    results = ParseResults<TResult>(result, query);

                   

                    lastBookmark = session.LastBookmark;
                    await session.CloseAsync();
                }
            }
            catch (AggregateException aggregateException)
            {
                context.Complete(query, lastBookmark, aggregateException.TryUnwrap(out var unwrappedException) ? unwrappedException : aggregateException);
                throw;
            }
            catch (Exception e)
            {
                context.Complete(query, lastBookmark, e);
                throw;
            }

            context.Complete(query, lastBookmark, results.Count, stats);
            return results;
        }

        private List<TResult> ParseResults<TResult>(IEnumerable<IRecord> result, CypherQuery query)
        {
            var deserializer = new CypherJsonDeserializer<TResult>(this, query.ResultMode, query.ResultFormat, false, true);
            var results = new List<TResult>();
            if (typeof(TResult).IsAnonymous())
            {
                foreach (var record in result)
                    results.AddRange(deserializer.Deserialize(record.ParseAnonymous(this), false));
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
            var executionContext = ExecutionContext.Begin(this);

            if (Driver == null)
                throw new InvalidOperationException("Can't execute cypher unless you have connected to the server.");

            if (InTransaction)
            {
                executionContext.Database = Transaction.Database;
                var response = await transactionManager.EnqueueCypherRequest($"The query was: {query.QueryText}", this, query).ConfigureAwait(false);
                QueryStats stats = null;
                if (query.IncludeQueryStats)
                {
                    var summary = await response.StatementResult.ConsumeAsync().ConfigureAwait(false);
                    stats = new QueryStats(summary.Counters);
                }

                OnOperationCompleted(new OperationCompletedEventArgs {QueryText = $"BOLT:{query.QueryText}", LastBookmark = transactionManager.LastBookmark, QueryStats = stats});
            }
            else
            {
                var session = Driver.AsyncSession(ServerVersion, query.Database, query.IsWrite, query.Bookmarks);
                IResultCursor cursor;
                if (query.IsWrite)
                    cursor = await session.WriteTransactionAsync(async s => await s.RunAsync(query, this)).ConfigureAwait(false);
                else
                    cursor = await session.ReadTransactionAsync(async s => await s.RunAsync(query, this)).ConfigureAwait(false);

                if (query.IncludeQueryStats)
                {
                    var summary = await cursor.ConsumeAsync().ConfigureAwait(false);
                    executionContext.Complete(query, session.LastBookmark, new QueryStats(summary.Counters));
                }
                else executionContext.Complete(query, session.LastBookmark);
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
            return BeginTransaction(TransactionScopeOption.Join, bookmarks, DefaultDatabase);
        }

        /// <inheritdoc />
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption)
        {
            return BeginTransaction(scopeOption, null, DefaultDatabase);
        }

        /// <inheritdoc />
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, string bookmark)
        {
            return BeginTransaction(scopeOption, new List<string>{bookmark}, DefaultDatabase);
        }

        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, IEnumerable<string> bookmark)
        {
            return BeginTransaction(scopeOption,  bookmark, DefaultDatabase);
        }


        /// <inheritdoc />
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, IEnumerable<string> bookmarks, string database)
        {
            return transactionManager.BeginTransaction(scopeOption, bookmarks, database);
        }

        /// <inheritdoc />
        public ITransaction Transaction => transactionManager?.CurrentTransaction;

        /// <inheritdoc />
        public bool InTransaction => transactionManager != null && transactionManager.InTransaction;

        /// <inheritdoc />
        public void EndTransaction()
        {
            transactionManager.EndTransaction();
        }

        /// <inheritdoc />
        public Uri TransactionEndpoint => throw new InvalidOperationException(NotValidForBolt);

        #endregion

        #region Implementation of IBoltGraphClient

        /// <inheritdoc cref="IGraphClient.OperationCompleted"/>
        public event OperationCompletedEventHandler OperationCompleted;

        /// <inheritdoc cref="IGraphClient.DefaultDatabase"/>
        public string DefaultDatabase { get; set; } = "neo4j";

        /// <inheritdoc cref="IGraphClient.CypherCapabilities"/>
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