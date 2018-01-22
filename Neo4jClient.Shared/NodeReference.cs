using System;
using System.Collections.Generic;
using System.Diagnostics;
using Neo4jClient.Cypher;
using Neo4jClient.Gremlin;

namespace Neo4jClient
{
    [DebuggerDisplay("Node {id}")]
    public class NodeReference : IGremlinQuery, IAttachedReference
    {
        readonly long id;
        readonly IGraphClient client;

        public NodeReference(long id) : this(id, null) {}

        public NodeReference(long id, IGraphClient client)
        {
            this.id = id;
            this.client = client;
        }

        public long Id { get { return id; } }

        public Type NodeType
        {
            get
            {
                var typedThis = this as ITypedNodeReference;
                return typedThis == null ? null : typedThis.NodeType;
            }
        }

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
            get { return "g.v(p0)"; }
        }

        IDictionary<string, object> IGremlinQuery.QueryParameters
        {
            get { return new Dictionary<string, object> {{"p0", Id}}; }
        }

        IList<string> IGremlinQuery.QueryDeclarations
        {
            get { return new List<string>(); }
        }

        public ICypherFluentQuery StartCypher(string identity)
        {
            var query = new CypherFluentQuery(client, true)
                .Start(identity, this);
            return query;
        }
    }
}
