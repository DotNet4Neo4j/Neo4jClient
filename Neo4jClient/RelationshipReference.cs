using System.Collections.Generic;
using System.Diagnostics;
using Neo4jClient.Gremlin;

namespace Neo4jClient
{
    [DebuggerDisplay("Relationship {id}")]
    public class RelationshipReference : IGremlinQuery, IAttachedReference
    {
        readonly long id;
        readonly IGraphClient client;

        public RelationshipReference(long id) : this(id, null) {}

        public RelationshipReference(long id, IGraphClient client)
        {
            this.id = id;
            this.client = client;
        }

        public long Id { get { return id; } }

        public static implicit operator RelationshipReference(long relationshipId)
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
            return id.GetHashCode();
        }

        IGraphClient IAttachedReference.Client
        {
            get { return client; }
        }

        string IGremlinQuery.QueryText
        {
            get { return "g.e(p0)"; }
        }

        IDictionary<string, object> IGremlinQuery.QueryParameters
        {
            get { return new Dictionary<string, object> { { "p0", Id }}; }
        }

        IList<string> IGremlinQuery.QueryDeclarations
        {
            get { return new List<string>(); }
        }
    }
}
