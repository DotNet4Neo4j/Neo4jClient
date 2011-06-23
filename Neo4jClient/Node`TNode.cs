using Neo4jClient.Gremlin;

namespace Neo4jClient
{
    public class Node<TNode> : IGremlinQuery
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

        IGraphClient IGremlinQuery.Client
        {
            get { return ((IGremlinQuery)reference).Client; }
        }

        string IGremlinQuery.QueryText
        {
            get { return ((IGremlinQuery)reference).QueryText; }
        }
    }
}