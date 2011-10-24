using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Neo4jClient.Gremlin
{
    [DebuggerDisplay("{DebugQueryText}")]
    internal class GremlinNodeEnumerable<TNode> : IGremlinNodeQuery<TNode>
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;

        public GremlinNodeEnumerable(IGremlinQuery query)
        {
            client = query.Client;
            queryText = query.QueryText;
            queryParameters = query.QueryParameters;
        }

        public string DebugQueryText
        {
            get
            {
                var text = queryText;
                foreach (var key in queryParameters.Keys.Reverse())
                {
                    text = text.Replace(key, string.Format("'{0}'", queryParameters[key]));
                }
                return text;
            }
        }

        IEnumerator<Node<TNode>> IEnumerable<Node<TNode>>.GetEnumerator()
        {
            if (client == null) throw new DetachedNodeException();
            return new GremlinPagedEnumerator<Node<TNode>>(
                client.ExecuteGetAllNodesGremlin<TNode>,
                new GremlinQuery(client, queryText, queryParameters));
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

        IDictionary<string, object> IGremlinQuery.QueryParameters
        {
            get { return queryParameters; }
        }
    }
}