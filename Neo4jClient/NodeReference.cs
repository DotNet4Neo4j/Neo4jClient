using System;
using Neo4jClient.Gremlin;

namespace Neo4jClient
{
    public class NodeReference : IGremlinQuery
    {
        public static readonly RootNode RootNode = new RootNode();

        readonly int id;
        readonly IGraphClient client;

        public NodeReference(int id) : this(id, null) {}

        public NodeReference(int id, IGraphClient client)
        {
            this.id = id;
            this.client = client;
        }

        public int Id { get { return id; } }

        public Type NodeType
        {
            get
            {
                var typedThis = this as ITypedNodeReference;
                return typedThis == null ? null : typedThis.NodeType;
            }
        }

        public static implicit operator NodeReference(int nodeId)
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
            return other.id == id;
        }

        public override int GetHashCode()
        {
            return id;
        }

        IGraphClient IGremlinQuery.Client
        {
            get { return client; }
        }

        string IGremlinQuery.QueryText
        {
            get { return string.Format("g.v({0})", Id); }
        }
    }
}