namespace Neo4jClient
{
    public class NodeReference
    {
        public static readonly RootNode RootNode = new RootNode();

        readonly int id;

        public NodeReference(int id)
        {
            this.id = id;
        }

        public int Id { get { return id; } }

        public static implicit operator NodeReference(int nodeId)
        {
            return new NodeReference(nodeId);
        }
    }
}