namespace Neo4jClient
{
// ReSharper disable UnusedTypeParameter
    public class OutgoingRelationship<T>
// ReSharper restore UnusedTypeParameter
    {
        readonly NodeReference target;

        public OutgoingRelationship(NodeReference target)
        {
            this.target = target;
        }

        public NodeReference Target { get { return target; } }
    }
}