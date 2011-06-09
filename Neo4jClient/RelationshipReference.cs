namespace Neo4jClient
{
    public class RelationshipReference
    {
        readonly int id;

        public RelationshipReference(int id)
        {
            this.id = id;
        }

        public int Id { get { return id; } }

        public static implicit operator RelationshipReference(int nodeId)
        {
            return new RelationshipReference(nodeId);
        }
    }
}