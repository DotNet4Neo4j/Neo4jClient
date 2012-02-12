using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{DebugQueryText}")]
    public class CypherQuery
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

        protected string DebugQueryText
        {
            get
            {
                var text = queryParameters
                    .Keys
                    .Aggregate(queryText, (current, paramName)
                        => current.Replace("{" + paramName + "}", queryParameters[paramName].ToString()))
                    .Replace("\r\n", "   ");

                return text;
            }
        }
    }
}
