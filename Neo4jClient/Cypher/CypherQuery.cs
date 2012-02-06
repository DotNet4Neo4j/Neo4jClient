using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public class CypherQuery : ICypherQuery
    {
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;

        public CypherQuery(string queryText, IDictionary<string, object> queryParameters)
        {
            this.queryText = queryText;
            this.queryParameters = queryParameters;
        }

        public IDictionary<string, object> QueryParameters
        {
            get { return queryParameters; }
        }

        public string QueryText
        {
            get { return queryText; }
        }
    }
}
