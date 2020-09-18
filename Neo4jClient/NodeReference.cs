using System;
using System.Diagnostics;

namespace Neo4jClient
{
    [DebuggerDisplay("Node {Id}")]
    public class NodeReference : IAttachedReference
    {
        private readonly IGraphClient client;

        public NodeReference(long id) : this(id, null)
        {
        }

        public NodeReference(long id, IGraphClient client)
        {
            this.Id = id;
            this.client = client;
        }

        public long Id { get; }

        public Type NodeType
        {
            get
            {
                var typedThis = this as ITypedNodeReference;
                return typedThis?.NodeType;
            }
        }

        IGraphClient IAttachedReference.Client => client;

        public static implicit operator NodeReference(long nodeId)
        {
            return new NodeReference(nodeId);
        }

        public static bool operator ==(NodeReference lhs, NodeReference rhs)
        {
            if (ReferenceEquals(lhs, null) && ReferenceEquals(rhs, null))
                return true;

            return !ReferenceEquals(lhs, null) && lhs.Equals(rhs);
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
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}