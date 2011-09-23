using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Neo4jClient.Gremlin
{
    [DebuggerDisplay("{queryText}")]
    internal class GremlinRelationshipEnumerable : IGremlinRelationshipQuery
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;

        public GremlinRelationshipEnumerable(IGraphClient client, string queryText, IDictionary<string, object> queryParameters)
        {
            this.client = client;
            this.queryText = queryText;
            this.queryParameters = queryParameters;
        }

        IEnumerator<RelationshipInstance> IEnumerable<RelationshipInstance>.GetEnumerator()
        {
            if (client == null) throw new DetachedNodeException();
            var results = client.ExecuteGetAllRelationshipsGremlin(queryText);
            return results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<RelationshipInstance>)this).GetEnumerator();
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