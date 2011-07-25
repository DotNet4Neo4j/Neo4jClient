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

        public static implicit operator RelationshipReference(int relationshipId)
        {
            return new RelationshipReference(relationshipId);
        }

        public static bool operator ==(RelationshipReference lhs, RelationshipReference rhs)
        {
            if (ReferenceEquals(lhs, null) && ReferenceEquals(rhs, null))
                return true;

            return !ReferenceEquals(lhs, null) && lhs.Equals(rhs);
        }

        public static bool operator !=(RelationshipReference lhs, RelationshipReference rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RelationshipReference;
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