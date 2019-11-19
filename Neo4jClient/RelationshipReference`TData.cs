using System.Diagnostics;

namespace Neo4jClient
{
    [DebuggerDisplay("Relationship {Id}")]
    public class RelationshipReference<TData> : RelationshipReference
    {
        public RelationshipReference(long id)
            : base(id)
        {
        }

        public RelationshipReference(long id, IGraphClient client)
            : base(id, client)
        {
        }
    }
}