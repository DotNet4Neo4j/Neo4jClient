using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Neo4jClient.Gremlin
{
    [DebuggerDisplay("{DebugQueryText}")]
    internal class GremlinNodeEnumerable<TNode> : IGremlinNodeQuery<TNode>
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;
        readonly IList<string> queryDeclarations;

        public GremlinNodeEnumerable(IGremlinQuery query)
        {
            client = query.Client;
            queryText = query.QueryText;
            queryParameters = query.QueryParameters;
            queryDeclarations = query.QueryDeclarations;
        }

        public string DebugQueryText
        {
            get { return this.ToDebugQueryText(); }
        }

        IEnumerator<Node<TNode>> IEnumerable<Node<TNode>>.GetEnumerator()
        {
            if (client == null) throw new DetachedNodeException();
            return new GremlinPagedEnumerator<Node<TNode>>(
                client.ExecuteGetAllNodesGremlin<TNode>,
                new GremlinQuery(client, queryText, queryParameters, queryDeclarations));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<NodeReference>)this).GetEnumerator();
        }

        IGraphClient IAttachedReference.Client
        {
            get { return client; }
        }

        string IGremlinQuery.QueryText
        {
            get { return queryText; }
        }

        IDictionary<string, object> IGremlinQuery.QueryParameters
        {
            get { return queryParameters; }
        }

        IList<string> IGremlinQuery.QueryDeclarations
        {
            get { return queryDeclarations; }
        }
    }
}