using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Neo4jClient.Serialization;

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

        CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer { JsonConverters = GraphClient.DefaultJsonConverters };
        }

        public string DebugQueryText
        {
            get
            {
                var text = queryParameters
                    .Keys
                    .Aggregate(
                        queryText,
                        (current, paramName) =>
                        {
                            var value = queryParameters[paramName];
                            var targetType = value.GetType();
                            if (targetType.IsClass && targetType != typeof (string))
                            {
                                value = BuildSerializer().Serialize(value);
                                value = Regex.Replace(
                                    value.ToString(),
                                    "\"\\w+\":",
                                    match =>
                                    {
                                        var val = match.ToString().Replace("\"", "");
                                        return val;
                                    });
                            }

                            return current.Replace("{" + paramName + "}", value.ToString());
                        });

                return text;
            }
        }
    }
}
