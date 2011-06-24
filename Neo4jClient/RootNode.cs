namespace Neo4jClient
{
    public class RootNode : NodeReference<RootNode>
    {
        public RootNode() : base(0) {}

        public RootNode(IGraphClient client) : base(0, client) {}
    }
}