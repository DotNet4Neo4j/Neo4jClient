using System;

namespace Neo4jClient.Execution
{
    internal class RelationshipIndexExecutionPolicy : RestExecutionPolicy
    {
        public RelationshipIndexExecutionPolicy(IGraphClient client)
            : base(client)
        {
        }

        public override Uri BaseEndpoint
        {
            get { return Client.RelationshipIndexEndpoint; }
        }
    }
}