using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    internal class GremlinQuery : IGremlinQuery
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;

        public GremlinQuery(IGraphClient client, string queryText, IDictionary<string, object> queryParameters)
        {
            this.client = client;
            this.queryText = queryText;
            this.queryParameters = queryParameters;
        }

        public IGraphClient Client
        {
            get { return client; }
        }

        public string QueryText
        {
            get { return queryText; }
        }

        public IDictionary<string, object> QueryParameters
        {
            get { return queryParameters; }
        }
    }
}