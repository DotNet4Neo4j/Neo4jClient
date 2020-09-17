using System;

namespace Neo4jClient
{
    public class Node<TNode> : IHasNodeReference, IAttachedReference
    {
        public Node(TNode data, NodeReference<TNode> reference)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            this.Data = data;
            this.Reference = reference;
        }

        public Node(TNode data, long id, IGraphClient client)
        {
            this.Data = data;
            Reference = new NodeReference<TNode>(id, client);
        }

        public NodeReference<TNode> Reference { get; }

        public TNode Data { get; }


        IGraphClient IAttachedReference.Client => ((IAttachedReference) Reference).Client;

        NodeReference IHasNodeReference.Reference => Reference;

        public static bool operator ==(Node<TNode> lhs, Node<TNode> rhs)
        {
            if (ReferenceEquals(lhs, null) && ReferenceEquals(rhs, null))
                return true;

            return !ReferenceEquals(lhs, null) && lhs.Equals(rhs);
        }

        public static bool operator !=(Node<TNode> lhs, Node<TNode> rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Node<TNode>;
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Reference == Reference;
        }

        public override int GetHashCode()
        {
            return Reference.Id.GetHashCode();
        }
    }
}