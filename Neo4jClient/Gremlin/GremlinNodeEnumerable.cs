using System.Collections;
using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    internal class GremlinNodeEnumerable<TNode> : IGremlinNodeQuery<TNode>
    {
        readonly IGraphClient client;
        readonly string queryText;

        public GremlinNodeEnumerable(IGraphClient client, string queryText)
        {
            this.client = client;
            this.queryText = queryText;
        }

        IEnumerator<Node<TNode>> IEnumerable<Node<TNode>>.GetEnumerator()
        {
            if (client == null) throw new DetachedNodeException();
            var results = client.ExecuteGetAllNodesGremlin<TNode>(queryText);
            return results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<NodeReference>)this).GetEnumerator();
        }

        IGraphClient IGremlinQuery.Client
        {
            get { return client; }
        }

        string IGremlinQuery.QueryText
        {
            get { return queryText; }
        }
    }
}