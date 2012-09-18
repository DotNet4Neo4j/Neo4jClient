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
        [Obsolete("You need to retrieve this via GraphClient.RootNode instead. This member will be removed in future builds.", true)]
        public static readonly RootNode RootNode = null;

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

        public ICypherFluentQueryStarted StartCypher(string identity)
        {
            var query = new CypherFluentQuery(client)
                .AddStartPoint(identity, this);
            return query;
        }
    }
}
