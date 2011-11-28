using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Neo4jClient.Gremlin
{
    [DebuggerDisplay("{DebugQueryText}")]
    internal class GremlinProjectionEnumerable<TResult> : IGremlinQuery, IEnumerable<TResult>
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;

        public GremlinProjectionEnumerable(IGremlinQuery query)
        {
            client = query.Client;
            queryText = query.QueryText;
            queryParameters = query.QueryParameters;
        }

        public string DebugQueryText
        {
            get { return this.ToDebugQueryText(); }
        }

        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
        {
            if (client == null) throw new DetachedNodeException();
            return client.ExecuteGetAllProjectionsGremlin<TResult>(new GremlinQuery(client, queryText, queryParameters))
                .GetEnumerator();
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
