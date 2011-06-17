using System;

namespace Neo4jClient
{
    public class NodeReference<TNode> : NodeReference, ITypedNodeReference
    {
        public NodeReference(int id)
            : base(id)
        {
        }

        Type ITypedNodeReference.NodeType
        {
            get { return typeof (TNode); }
        }
    }
}