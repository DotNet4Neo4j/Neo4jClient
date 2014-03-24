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
        readonly CypherResultMode resultMode;

        public CypherQuery(
            string queryText,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode)
        {
            this.queryText = queryText;
            this.queryParameters = queryParameters;
            this.resultMode = resultMode;
        }

        public IDictionary<string, object> QueryParameters
        {
            get { return queryParameters; }
        }

        public string QueryText
        {
            get { return queryText; }
        }

        public CypherResultMode ResultMode
        {
            get { return resultMode; }
        }

        public string DebugQueryText
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
