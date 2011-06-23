using System;

namespace Neo4jClient
{
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