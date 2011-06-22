namespace Neo4jClient
{
    public class Node<TNode>
    {
        readonly TNode data;
        readonly NodeReference<TNode> reference;

        public Node(TNode data, NodeReference<TNode> reference)
        {
            this.data = data;
            this.reference = reference;
        }

        public NodeReference<TNode> Reference
        {
            get { return reference; }
        }

        public TNode Data
        {
            get { return data; }
        }
    }
}