using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;

namespace Neo4jClient.Execution
{
    /// <summary>
    /// Describes the behavior for a cypher execution.
    /// </summary>
    internal class CypherExecutionPolicy : GraphClientBasedExecutionPolicy
    {
        public CypherExecutionPolicy(IGraphClient client) : base(client)
        {
        }

        private INeo4jTransaction GetTransactionInScope()
        {
            // first try to get the Non DTC transaction and if it doesn't succeed then try it with the DTC
            if (!(Client is IInternalTransactionalGraphClient<HttpResponseMessage> transactionalClient))
                return null;

            if (transactionalClient.Transaction is TransactionScopeProxy proxiedTransaction)
                return proxiedTransaction.TransactionContext;

            return null;
        }

        public override Uri BaseEndpoint(string database = null, bool autoCommit = false)
        {
            if (!InTransaction && Client.TransactionEndpoint != null)
                return Client.GetTransactionEndpoint(database, autoCommit);

            var proxiedTransaction = GetTransactionInScope();
            var transactionalClient = (ITransactionalGraphClient) Client;
            if (proxiedTransaction == null)
            {
                return Replace(transactionalClient.TransactionEndpoint, database);
            }

            var startingReference = proxiedTransaction.Endpoint ?? Client.GetTransactionEndpoint(database, autoCommit);
            return startingReference;
        }



        public override TransactionExecutionPolicy TransactionExecutionPolicy => TransactionExecutionPolicy.Allowed;

        public override string SerializeRequest(object toSerialize)
        {
            if (!(toSerialize is CypherQuery query))
                throw new InvalidOperationException("Unsupported operation: Attempting to serialize something that was not a query.");

            return Client
                .Serializer
                .Serialize(new CypherStatementList
                {
                    new CypherTransactionStatement(query)
                });
        }

        public override string Database { get; set; }

        public override void AfterExecution(IDictionary<string, object> executionMetadata, object executionContext)
        {
            if (Client == null || executionMetadata == null || executionMetadata.Count == 0)
                return;

            // determine if we need to update the transaction end point
            if (!(executionContext is INeo4jTransaction transaction) || transaction.Endpoint != null)
                return;

            if (!executionMetadata.TryGetValue("Location", out var locationValue))
                return;

            if (!(locationValue is IEnumerable<string> locationHeader))
                return;

            var generatedEndpoint = locationHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(generatedEndpoint))
                transaction.Endpoint = new Uri(generatedEndpoint);
        }
    }
}