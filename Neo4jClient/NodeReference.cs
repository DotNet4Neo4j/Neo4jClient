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

        public static bool operator ==(NodeReference lhs, NodeReference rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(NodeReference lhs, NodeReference rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            var other = obj as NodeReference;
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.id == id;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}