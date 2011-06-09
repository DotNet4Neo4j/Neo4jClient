namespace Neo4jClient
{
    public class NodeReference
    {
        public static readonly NodeReference RootNode = new NodeReference(0);

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