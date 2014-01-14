using System;

namespace Neo4jClient.Execution
{
    internal class NodeIndexExecutionPolicy : RestExecutionPolicy
    {
        public NodeIndexExecutionPolicy(IGraphClient client)
            : base(client)
        {
        }

        public override Uri BaseEndpoint
        {
            get { return Client.NodeIndexEndpoint; }
        }
    }
}