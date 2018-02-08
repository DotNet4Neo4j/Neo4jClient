using System;
using System.Collections.Specialized;
using System.Net;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Implements the Neo4j HTTP transaction for multiple HTTP requests
    /// </summary>
    internal class Neo4jRestTransaction: INeo4jTransaction
    {
        private readonly ITransactionalGraphClient client;

        public bool IsOpen { get; private set; }

        public Uri Endpoint { get; set; }
        public NameValueCollection CustomHeaders { get; set; }

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

        internal static Neo4jRestTransaction FromIdAndClient(int transactionId, ITransactionalGraphClient client)
        {
            return new Neo4jRestTransaction(client)
            {
                Endpoint = client.TransactionEndpoint.AddPath(transactionId.ToString())
            };
        }

        public Neo4jRestTransaction(ITransactionalGraphClient graphClient)
        {
            Endpoint = null;
            IsOpen = true;
            client = graphClient;
        }

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
        public void Commit()
        {
            CheckForOpenTransaction();
            // we have to check for an empty endpoint because we dont have one until our first request
            if (Endpoint == null)
            {
                CleanupAfterClosedTransaction();
                return;
            }

            DoCommit(Endpoint, client.ExecutionConfiguration, client.Serializer, CustomHeaders);
            CleanupAfterClosedTransaction();
        }

        /// <summary>
        /// Rolls back our current transaction and closes the transaction.
        /// </summary>
        public void Rollback()
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

            Request.With(client.ExecutionConfiguration)
                .Delete(Endpoint)
                .WithExpectedStatusCodes(expectedStatusCodes)
                .Execute();

            CleanupAfterClosedTransaction();
        }

        /// <summary>
        /// Emits an empty request to keep alive our current transaction.
        /// </summary>
        public void KeepAlive()
        {
            CheckForOpenTransaction();
            // no need to issue a request as we haven't sent a single request
            if (Endpoint == null)
            {
                return;
            }

            DoKeepAlive(Endpoint, client.ExecutionConfiguration, client.Serializer);
        }

        /// <summary>
        /// Forces a keep alive, setting the endpoint if necessary
        /// </summary>
        internal void ForceKeepAlive()
        {
            var keepAliveUri = Endpoint ?? client.TransactionEndpoint;
            var transactionEndpoint = DoKeepAlive(
                keepAliveUri, 
                client.ExecutionConfiguration,
                client.Serializer,newTransaction: Endpoint == null);
            
            if (Endpoint != null)
            {
                return;
            }
            Endpoint = transactionEndpoint;
        }

        private static void DoCommit(Uri commitUri, ExecutionConfiguration executionConfiguration, ISerializer serializer, NameValueCollection customHeaders = null)
        {
            Request.With(executionConfiguration, customHeaders)
               .Post(commitUri.AddPath("commit"))
               .WithJsonContent(serializer.Serialize(new CypherStatementList()))
               .WithExpectedStatusCodes(HttpStatusCode.OK)
               .Execute();
        }

        private static void DoRollback(Uri rollbackUri, ExecutionConfiguration executionConfiguration, NameValueCollection customHeaders)
        {
            // not found is ok because it means our transaction either was committed or the timeout was expired
            // and it was rolled back for us
            Request.With(executionConfiguration, customHeaders)
                .Delete(rollbackUri)
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.NotFound)
                .Execute();
        }

        private static Uri DoKeepAlive(
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
                partialRequest.WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.Created).Execute() :
                partialRequest.WithExpectedStatusCodes(HttpStatusCode.OK).Execute();
            
            return response.Headers.Location;
        }

        /// <summary>
        /// Commits a transaction given the ID
        /// </summary>
        /// <param name="transactionExecutionEnvironment">The transaction execution environment</param>
        /// <param name="customHeaders">Custom headers to sent to the neo4j server</param>
        internal static void DoCommit(ITransactionExecutionEnvironment transactionExecutionEnvironment, NameValueCollection customHeaders = null)
        {
            var commitUri = transactionExecutionEnvironment.TransactionBaseEndpoint.AddPath(
                transactionExecutionEnvironment.TransactionId.ToString());
            
            DoCommit(
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
        internal static void DoRollback(ITransactionExecutionEnvironment transactionExecutionEnvironment, NameValueCollection customHeaders = null)
        {
            try
            {
                var rollbackUri = transactionExecutionEnvironment.TransactionBaseEndpoint.AddPath(
                    transactionExecutionEnvironment.TransactionId.ToString());
                DoRollback(
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
        internal static void DoKeepAlive(ITransactionExecutionEnvironment transactionExecutionEnvironment)
        {
            var keepAliveUri = transactionExecutionEnvironment.TransactionBaseEndpoint.AddPath(
                transactionExecutionEnvironment.TransactionId.ToString());
            DoKeepAlive(
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
                Rollback();
            }
        }
    }
}
