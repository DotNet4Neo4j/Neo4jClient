using System;
using System.Diagnostics;
using System.Reflection;

namespace Neo4jClient
{
    [DebuggerDisplay("Node {Id}")]
    public class NodeReference<TNode> : NodeReference, ITypedNodeReference
    {
        public NodeReference(long id)
            : base(id)
        {
            CheckTNode();
        }

        public NodeReference(long id, IGraphClient client)
            : base(id, client)
        {
            CheckTNode();
        }

        static void CheckTNode()
        {
            var type = typeof (TNode);
            if (!type.GetTypeInfo().IsGenericType) return;
            if (type.GetGenericTypeDefinition() != typeof(Node<>)) return;

            throw new NotSupportedException(string.Format(
                "You're trying to initialize NodeReference<Node<{0}>> which is too many levels of nesting. You should just be using NodeReference<{0}> instead. (You use a Node, or a NodeReference, but not both together.)",
                type.GetGenericArguments()[0].FullName
            ));
        }

        Type ITypedNodeReference.NodeType
        {
            get { return typeof (TNode); }
        }

        public static implicit operator NodeReference<TNode>(long nodeId)
        {
            return new NodeReference<TNode>(nodeId);
        }
    }
}