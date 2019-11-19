using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public class IdentityPipe : IGremlinQuery
    {
        readonly IDictionary<string, object> queryParameters = new Dictionary<string, object>();
        readonly IList<string> queryDeclarations = new List<string>();

        public IGraphClient Client { get; set; }

        public string QueryText
        {
            get { return "_()"; }
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