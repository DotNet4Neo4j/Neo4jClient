using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    internal class IdentityPipe : IGremlinQuery
    {
        readonly IDictionary<string, object> queryParameters = new Dictionary<string, object>();

        public IGraphClient Client { get; set; }

        public string QueryText
        {
            get { return "_()"; }
        }

        public IDictionary<string, object> QueryParameters
        {
            get { return queryParameters; }
        }
    }
}