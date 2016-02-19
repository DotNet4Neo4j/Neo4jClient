using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    internal class GremlinQuery : IGremlinQuery
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;
        readonly IList<string> queryDeclarations;

        public GremlinQuery(IGraphClient client, string queryText, IDictionary<string, object> queryParameters, IList<string> declarations )
        {
            this.client = client;
            this.queryText = queryText;
            this.queryParameters = queryParameters;
            this.queryDeclarations = declarations;
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

        public IList<string> QueryDeclarations
        {
            get { return queryDeclarations; }
        }
    }
}