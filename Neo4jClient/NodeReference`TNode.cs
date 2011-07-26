using System;
using System.Diagnostics;

namespace Neo4jClient
{
    [DebuggerDisplay("Node {Id}")]
    public class NodeReference<TNode> : NodeReference, ITypedNodeReference
    {
        public NodeReference(int id)
            : base(id)
        {
        }

        public NodeReference(int id, IGraphClient client)
            : base(id, client)
        {
        }

        Type ITypedNodeReference.NodeType
        {
            get { return typeof (TNode); }
        }
    }
}