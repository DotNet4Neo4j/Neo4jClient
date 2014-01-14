using System;
using System.Net;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Execution;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Implements the Neo4j HTTP transaction for multiple HTTP requests
    /// </summary>
    class Neo4jTransaction : INeo4jTransaction
    {
        private readonly ITransactionalGraphClient _client;

        public bool IsOpen { get; private set; }

        public Uri Endpoint { get; set; }

        public Neo4jTransaction(ITransactionalGraphClient graphClient)
        {
            Endpoint = null;
            IsOpen = true;
            _client = graphClient;
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

            Request.With(_client.ExecutionConfiguration)
                .Post(Endpoint.AddPath("commit"))
                .WithJsonContent(_client.Serializer.Serialize(new CypherStatementList()))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .Execute();

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

            Request.With(_client.ExecutionConfiguration)
                .Delete(Endpoint)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
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

            Request.With(_client.ExecutionConfiguration)
                .Post(Endpoint)
                .WithJsonContent(_client.Serializer.Serialize(new CypherStatementList()))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .Execute();
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
