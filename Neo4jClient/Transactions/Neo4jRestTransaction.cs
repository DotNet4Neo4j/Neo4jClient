using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;

namespace Neo4jClient.Transactions
{
    using Neo4j.Driver;

    /// <summary>
    /// Implements the Neo4j HTTP transaction for multiple HTTP requests
    /// </summary>
    internal class Neo4jRestTransaction: INeo4jTransaction
    {
        private readonly ITransactionalGraphClient client;

        public bool IsOpen { get; private set; }

        public Uri Endpoint { get; set; }
        public NameValueCollection CustomHeaders { get; set; }
        public Bookmark LastBookmark => throw new InvalidOperationException("This is not possible with the GraphClient. You would need the BoltGraphClient.");

        internal int Id
        {
            get
            {
                if (Endpoint == null)
                {
                    throw new InvalidOperationException("Id is unknown at this point");
                }

                var transactionEndpoint = client.TransactionEndpoint.ToString();
                int transactionEndpointLength = transactionEndpoint.Length;
                if (!transactionEndpoint.EndsWith("/"))
                {
                    transactionEndpointLength++;
                }
                return int.Parse(Endpoint.ToString().Substring(transactionEndpointLength));
            }
        }

        public Neo4jRestTransaction(ITransactionalGraphClient graphClient, string database)
        {
            Endpoint = null;
            IsOpen = true;
            client = graphClient;
            Database = database;
        }

        public string Database { get; set; }

        protected void CleanupAfterClosedTransaction()
        {
            IsOpen = false;
        }

        private void CheckForOpenTransaction()
        {
            if (IsOpen)
            {
                return;
            }

            string endPointText = null;
            if (Endpoint != null)
            {
                endPointText = Endpoint.ToString();
            }
            throw new ClosedTransactionException(endPointText);
        }

        /// <summary>
        /// Cancels a transaction without closing it in the server
        /// </summary>
        internal void Cancel()
        {
            Endpoint = null;
            IsOpen = false;
        }

        /// <summary>
        /// Commits our current transaction and closes the transaction.
        /// </summary>
        public async Task CommitAsync()
        {
            CheckForOpenTransaction();
            // we have to check for an empty endpoint because we dont have one until our first request
            if (Endpoint == null)
            {
                CleanupAfterClosedTransaction();
                return;
            }

            await DoCommit(Endpoint, client.ExecutionConfiguration, client.Serializer, CustomHeaders).ConfigureAwait(false);
            CleanupAfterClosedTransaction();
        }

        /// <summary>
        /// Rolls back our current transaction and closes the transaction.
        /// </summary>
        public async Task RollbackAsync()
        {
            CheckForOpenTransaction();
            // we have to check for an empty endpoint because we dont have one until our first request
            
            if (Endpoint == null)
            {
                CleanupAfterClosedTransaction();
                return;
            }
            
            //This change is due to: https://github.com/Readify/Neo4jClient/issues/127 and https://github.com/neo4j/neo4j/issues/5806 - 
            HttpStatusCode[] expectedStatusCodes = {HttpStatusCode.OK};
            if (client.CypherCapabilities.AutoRollsBackOnError && client.ExecutionConfiguration.HasErrors)
                    expectedStatusCodes = new [] {HttpStatusCode.OK, HttpStatusCode.NotFound};

            await Request.With(client.ExecutionConfiguration)
                .Delete(Endpoint)
                .WithExpectedStatusCodes(expectedStatusCodes)
                .ExecuteAsync().ConfigureAwait(false);

            CleanupAfterClosedTransaction();
        }

        /// <summary>
        /// Emits an empty request to keep alive our current transaction.
        /// </summary>
        public Task KeepAliveAsync()
        {
            CheckForOpenTransaction();
            // no need to issue a request as we haven't sent a single request
            if (Endpoint == null)
            {
#if NET45
                return Task.FromResult(0);
#else
                return Task.CompletedTask;
#endif
            }

            return DoKeepAlive(Endpoint, client.ExecutionConfiguration, client.Serializer);
        }

        /// <summary>
        /// Forces a keep alive, setting the endpoint if necessary
        /// </summary>
        internal async Task ForceKeepAlive()
        {
            var keepAliveUri = Endpoint ?? client.TransactionEndpoint;
            var transactionEndpoint = await DoKeepAlive(
                keepAliveUri, 
                client.ExecutionConfiguration,
                client.Serializer,newTransaction: Endpoint == null).ConfigureAwait(false);
            
            if (Endpoint != null)
            {
                return;
            }
            Endpoint = transactionEndpoint;
        }

        private static Task DoCommit(Uri commitUri, ExecutionConfiguration executionConfiguration, ISerializer serializer, NameValueCollection customHeaders = null)
        {
            return Request.With(executionConfiguration, customHeaders)
               .Post(commitUri.AddPath("commit"))
               .WithJsonContent(serializer.Serialize(new CypherStatementList()))
               .WithExpectedStatusCodes(HttpStatusCode.OK)
               .ExecuteAsync();
        }

        private static Task DoRollback(Uri rollbackUri, ExecutionConfiguration executionConfiguration, NameValueCollection customHeaders)
        {
            // not found is ok because it means our transaction either was committed or the timeout was expired
            // and it was rolled back for us
            return Request.With(executionConfiguration, customHeaders)
                .Delete(rollbackUri)
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NotFound)
                .ExecuteAsync();
        }

        private static async Task<Uri> DoKeepAlive(
            Uri keepAliveUri,
            ExecutionConfiguration executionConfiguration,
            ISerializer serializer,
            NameValueCollection customHeaders = null,
            bool newTransaction = false)
        {
            var partialRequest = Request.With(executionConfiguration, customHeaders)
                .Post(keepAliveUri)
                .WithJsonContent(serializer.Serialize(new CypherStatementList()));
            
            var response = newTransaction ?
                await partialRequest.WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.Created).ExecuteAsync().ConfigureAwait(false) :
                await partialRequest.WithExpectedStatusCodes(HttpStatusCode.OK).ExecuteAsync().ConfigureAwait(false);
            
            return response.Headers.Location;
        }

        /// <summary>
        /// Commits a transaction given the ID
        /// </summary>
        /// <param name="transactionExecutionEnvironment">The transaction execution environment</param>
        /// <param name="customHeaders">Custom headers to sent to the neo4j server</param>
        internal static Task DoCommit(ITransactionExecutionEnvironment transactionExecutionEnvironment, NameValueCollection customHeaders = null)
        {
            var commitUri = transactionExecutionEnvironment.TransactionBaseEndpoint.AddPath(
                transactionExecutionEnvironment.TransactionId.ToString());
            
            return DoCommit(
                commitUri,
                new ExecutionConfiguration
                {
                    HttpClient =  new HttpClientWrapper(transactionExecutionEnvironment.Username, transactionExecutionEnvironment.Password),
                    JsonConverters = GraphClient.DefaultJsonConverters,
                    UseJsonStreaming =  transactionExecutionEnvironment.UseJsonStreaming,
                    UserAgent = transactionExecutionEnvironment.UserAgent
                },
                new CustomJsonSerializer(),
                customHeaders
                );
        }

        /// <summary>
        /// Rolls back a transaction given the ID
        /// </summary>
        /// <param name="transactionExecutionEnvironment">The transaction execution environment</param>
        internal static Task DoRollback(ITransactionExecutionEnvironment transactionExecutionEnvironment, NameValueCollection customHeaders = null)
        {
            try
            {
                var rollbackUri = transactionExecutionEnvironment.TransactionBaseEndpoint.AddPath(
                    transactionExecutionEnvironment.TransactionId.ToString());
                return DoRollback(
                    rollbackUri,
                    new ExecutionConfiguration
                    {
                        HttpClient = new HttpClientWrapper(transactionExecutionEnvironment.Username, transactionExecutionEnvironment.Password),
                        JsonConverters = GraphClient.DefaultJsonConverters,
                        UseJsonStreaming = transactionExecutionEnvironment.UseJsonStreaming,
                        UserAgent = transactionExecutionEnvironment.UserAgent
                    },
                    customHeaders);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Keeps alive a transaction given the ID
        /// </summary>
        /// <param name="transactionExecutionEnvironment">The transaction execution environment</param>
        internal static Task DoKeepAlive(ITransactionExecutionEnvironment transactionExecutionEnvironment)
        {
            var keepAliveUri = transactionExecutionEnvironment.TransactionBaseEndpoint.AddPath(
                transactionExecutionEnvironment.TransactionId.ToString());
            return DoKeepAlive(
                keepAliveUri,
                new ExecutionConfiguration
                {
                    HttpClient =
                        new HttpClientWrapper(transactionExecutionEnvironment.Username,
                            transactionExecutionEnvironment.Password),
                    JsonConverters = GraphClient.DefaultJsonConverters,
                    UseJsonStreaming = transactionExecutionEnvironment.UseJsonStreaming,
                    UserAgent = transactionExecutionEnvironment.UserAgent
                }, new CustomJsonSerializer(), null);
        }

        public void Dispose()
        {
            if (IsOpen)
            {
                RollbackAsync().Wait(); // annoying but can't dispose asynchronously
            }
        }
    }
}
