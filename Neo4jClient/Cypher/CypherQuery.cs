using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Neo4jClient.Cypher
{
    public enum CypherResultFormat
    {
        Rest,
        Transactional,
        DependsOnEnvironment
    }

    [DebuggerDisplay("{DebugQueryText}")]
    public class CypherQuery
    {
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;
        readonly CypherResultMode resultMode;
        private readonly CypherResultFormat resultFormat;

        public CypherQuery(
            string queryText,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode) :
            this(queryText, queryParameters, resultMode, CypherResultFormat.DependsOnEnvironment)
        {
        }

        public CypherQuery(
            string queryText,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode,
            CypherResultFormat resultFormat)
        {
            this.queryText = queryText;
            this.queryParameters = queryParameters;
            this.resultMode = resultMode;
            this.resultFormat = resultFormat;
        }

        public IDictionary<string, object> QueryParameters
        {
            get { return queryParameters; }
        }

        public string QueryText
        {
            get { return queryText; }
        }

        public CypherResultFormat ResultFormat
        {
            get { return resultFormat; }
        }

        public CypherResultMode ResultMode
        {
            get { return resultMode; }
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
