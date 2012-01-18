using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{DebugQueryText}")]
    internal class CypherNodeEnumerable<TNode> : ICypherNodeQuery<TNode>
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;
        readonly IList<string> queryDeclarations;

        public CypherNodeEnumerable(ICypherQuery query)
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
            return new CypherPagedEnumerator<Node<TNode>>(
                client.ExecuteGetAllNodesCypher<TNode>,
                new CypherQuery(client, queryText, queryParameters, queryDeclarations));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<NodeReference>)this).GetEnumerator();
        }

        IGraphClient ICypherQuery.Client
        {
            get { return client; }
        }

        string ICypherQuery.QueryText
        {
            get { return queryText; }
        }

        IDictionary<string, object> ICypherQuery.QueryParameters
        {
            get { return queryParameters; }
        }

        IList<string> ICypherQuery.QueryDeclarations
        {
            get { return queryDeclarations; }
        }
    }
}