using Neo4jClient.Cypher;

namespace Neo4jClient
{
    public class RootNode : NodeReference<RootNode>
    {
        public RootNode() : base(0) {}

        public RootNode(IGraphClient client) : base(0, client) {}

        public ICypherFluentQueryStarted StartCypher(string identity)
        {
            var client = ((IAttachedReference)this).Client;
            var query = new CypherFluentQuery(client)
                .AddStartPoint(identity, this);
            return query;
        }
    }
}