using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Execution
{
    class ExecutionPolicyFactory : IExecutionPolicyFactory
    {
        private IGraphClient _client;

        public ExecutionPolicyFactory(IGraphClient client)
        {
            _client = client;
        }

        public IExecutionPolicy GetPolicy(PolicyType type)
        {
            if (!_client.IsConnected)
            {
                throw new InvalidOperationException("Client has not connected to the Neo4j server");
            }

            // todo: this should be a prototype-based object creation
            switch (type)
            {
                case PolicyType.Cypher:
                    return new CypherExecutionPolicy(_client);
                case PolicyType.Gremlin:
                    return new GremlinExecutionPolicy(_client);
                case PolicyType.Batch:
                    return new BatchExecutionPolicy(_client);
                case PolicyType.Rest:
                    return new RestExecutionPolicy(_client);
                case PolicyType.Transaction:
                    return new CypherTransactionExecutionPolicy(_client);
                case PolicyType.NodeIndex:
                    return new NodeIndexExecutionPolicy(_client);
                case PolicyType.RelationshipIndex:
                    return new RelationshipIndexExecutionPolicy(_client);
                default:
                    throw new InvalidOperationException("Unknown execution policy:" + type.ToString());
            }
        }
    }
}
