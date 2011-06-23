using System;
using System.Collections;

namespace Neo4jClient.Gremlin
{
    internal class GremlinReferenceEnumerable : IGremlinReferenceQuery
    {
        readonly IGraphClient client;
        readonly string queryText;

        public GremlinReferenceEnumerable(IGraphClient client, string queryText)
        {
            this.client = client;
            this.queryText = queryText;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
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