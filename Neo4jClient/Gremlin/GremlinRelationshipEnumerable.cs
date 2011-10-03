using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Neo4jClient.Gremlin
{
    [DebuggerDisplay("{DebugQueryText}")]
    internal class GremlinRelationshipEnumerable : IGremlinRelationshipQuery
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;

        public GremlinRelationshipEnumerable(IGremlinQuery query)
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

        IEnumerator<RelationshipInstance> IEnumerable<RelationshipInstance>.GetEnumerator()
        {
            if (client == null) throw new DetachedNodeException();
            var results = client.ExecuteGetAllRelationshipsGremlin(queryText, queryParameters);
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