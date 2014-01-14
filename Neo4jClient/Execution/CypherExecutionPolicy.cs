using System;
using System.Collections.Generic;
using System.Linq;
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

        public override Uri BaseEndpoint
        {
            get
            {
                if (!InTransaction)
                {
                    return Client.CypherEndpoint;
                }

                var transactionalClient = (ITransactionalGraphClient) Client;
                var startingReference = ((INeo4jTransaction) transactionalClient.Transaction).Endpoint ??
                                        transactionalClient.TransactionEndpoint;
                return startingReference;
            }
        }

        public override TransactionExecutionPolicy TransactionExecutionPolicy
        {
            get { return TransactionExecutionPolicy.Allowed; }
        }

        public override string SerializeRequest(object toSerialize)
        {
            var query = toSerialize as CypherQuery;
            if (toSerialize == null)
            {
                throw new InvalidOperationException(
                    "Unsupported operation: Attempting to serialize something that was not a query.");
            }

            if (InTransaction)
            {
                return Client.Serializer.Serialize(new CypherStatementList {new CypherTransactionStatement(query)});
            }
            return Client.Serializer.Serialize(new CypherApiQuery(query));
        }

        public override void AfterExecution(IDictionary<string, object> executionMetadata)
        {
            if (Client == null || executionMetadata == null || executionMetadata.Count == 0)
            {
                return;
            }

            var transactionalClient = Client as ITransactionalGraphClient;
            if (!InTransaction || transactionalClient == null)
            {
                return;
            }

            // determine if we need to update the transaction end point
            var transaction = (INeo4jTransaction) transactionalClient.Transaction;
            if (transaction.Endpoint != null) return;
            var locationHeader = executionMetadata["Location"] as IEnumerable<string>;
            if (locationHeader == null)
            {
                return;
            }

            var generatedEndpoint = locationHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(generatedEndpoint))
            {
                transaction.Endpoint = new Uri(generatedEndpoint);
            }
        }

    }
}
