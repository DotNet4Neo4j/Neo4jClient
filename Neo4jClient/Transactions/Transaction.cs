using System;
using System.Net;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Execution;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Implements the Neo4j HTTP transaction for multiple HTTP requests
    /// </summary>
    class Transaction : INeo4jTransaction
    {
        private bool _isOpen;
        private bool _disposing;
        private readonly ITransactionalGraphClient _graphClient;
        private readonly IExecutionPolicyFactory _policyFactory;

        public Uri Endpoint { get; set; }

        public Transaction(ITransactionalGraphClient graphClient)
            : this(graphClient, new ExecutionPolicyFactory(graphClient))
        {
        }

        public Transaction(ITransactionalGraphClient graphClient, IExecutionPolicyFactory policyFactory)
        {
            _isOpen = true;
            Endpoint = null;
            _graphClient = graphClient;
            _disposing = false;
            _policyFactory = policyFactory;
        }

        private void CheckForOpenTransaction()
        {
            if (_isOpen)
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

        private void CleanupAfterClosedTransaction()
        {
            _isOpen = false;
            _graphClient.EndTransaction();
            _disposing = false;
        }

        /// <summary>
        /// Dispose our current transaction, rolling back if it is still open.
        /// </summary>
        public void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            if (_isOpen && Endpoint != null)
            {
                Rollback();
            }
            else
            {
                _disposing = true;
                CleanupAfterClosedTransaction();
            }
        }

        /// <summary>
        /// Commits our current transaction and closes the transaction.
        /// </summary>
        public void Commit()
        {
            if (_disposing)
            {
                return;
            }
            _disposing = true;

            CheckForOpenTransaction();
            // we have to check for an empty endpoint because we dont have one until our first request
            if (Endpoint == null)
            {
                CleanupAfterClosedTransaction();
                return;
            }

            var policy = _policyFactory.GetPolicy(PolicyType.Cypher);
            Request.With(_graphClient.ExecutionConfiguration)
                .Post(policy.BaseEndpoint.AddPath("commit"))
                .WithJsonContent(_graphClient.Serializer.Serialize(new CypherStatementList()))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .Execute();

            CleanupAfterClosedTransaction();
        }

        /// <summary>
        /// Rolls back our current transaction and closes the transaction.
        /// </summary>
        public void Rollback()
        {
            if (_disposing)
            {
                return;
            }
            _disposing = true;

            CheckForOpenTransaction();
            // we have to check for an empty endpoint because we dont have one until our first request
            if (Endpoint == null)
            {
                CleanupAfterClosedTransaction();
                return;
            }

            var policy = _policyFactory.GetPolicy(PolicyType.Cypher);
            Request.With(_graphClient.ExecutionConfiguration)
                .Delete(policy.BaseEndpoint)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .Execute();

            CleanupAfterClosedTransaction();
        }

        /// <summary>
        /// Emits an empty request to keep alive our current transaction.
        /// </summary>
        public void KeepAlive()
        {
            if (_disposing)
            {
                return;
            }

            CheckForOpenTransaction();
            // no need to issue a request as we haven't sent a single request
            if (Endpoint == null)
            {
                return;
            }

            var policy = _policyFactory.GetPolicy(PolicyType.Cypher);
            Request.With(_graphClient.ExecutionConfiguration)
                .Post(policy.BaseEndpoint)
                .WithJsonContent(_graphClient.Serializer.Serialize(new CypherStatementList()))
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .Execute();
        }
    }
}
