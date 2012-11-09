namespace Neo4jClient
{
    public class RootNode : NodeReference<RootNode>
    {
        public RootNode() : base(0) { }
        public RootNode(long id) : base(id) {}
        public RootNode(long id, IGraphClient client) : base(id, client) {}
    }
}
