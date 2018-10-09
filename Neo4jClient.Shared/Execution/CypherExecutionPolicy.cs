using System;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;

//using Neo4jClient.Transactions;


namespace Neo4jClient.Execution
{
    /// <summary>
    /// Describes the behavior for a cypher execution.
    /// </summary>
    internal partial class CypherExecutionPolicy : GraphClientBasedExecutionPolicy
    {
        public CypherExecutionPolicy(IGraphClient client) : base(client)
        {
        }




        public override string SerializeRequest(object toSerialize)
        {
            var query = toSerialize as CypherQuery;
            if (query == null)
            {
                throw new InvalidOperationException(
                    "Unsupported operation: Attempting to serialize something that was not a query.");
            }

            if (InTransaction)
            {
                return Client
                    .Serializer
                    .Serialize(new CypherStatementList
                    {
                        new CypherTransactionStatement(query, query.ResultFormat == CypherResultFormat.Rest)
                    });
            }
            return Client.Serializer.Serialize(new CypherApiQuery(query));
        }

        public override void AfterExecution(IDictionary<string, object> executionMetadata, object executionContext)
        {
            if (Client == null || executionMetadata == null || executionMetadata.Count == 0)
            {
                return;
            }

            // determine if we need to update the transaction end point
            var transaction = executionContext as INeo4jTransaction;
            if (transaction == null || transaction.Endpoint != null)
            {
                return;
            }

            object locationValue;
            if (!executionMetadata.TryGetValue("Location", out locationValue))
            {
                return;
            }

            var locationHeader = locationValue as IEnumerable<string>;
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
