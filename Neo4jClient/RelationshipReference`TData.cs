using System.Diagnostics;

namespace Neo4jClient
{
    [DebuggerDisplay("Relationship {Id}")]
    public class RelationshipReference<TData> : RelationshipReference
    {
        public RelationshipReference(int id)
            : base(id)
        {
        }

        public RelationshipReference(int id, IGraphClient client)
            : base(id, client)
        {
        }
    }
}