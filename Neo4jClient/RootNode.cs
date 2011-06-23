namespace Neo4jClient
{
    public class RootNode : NodeReference<RootNode>
    {
        internal RootNode() : base(0) {}

        internal RootNode(IGraphClient client) : base(0, client) {}
    }
}