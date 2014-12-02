using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{DebugQueryText}")]
    public class CypherQuery
    {
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;
        readonly CypherResultMode resultMode;
        readonly IContractResolver jsonContractResolver;
        readonly int? maxExecutionTime;

        public CypherQuery(
            string queryText,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode, 
            IContractResolver contractResolver = null,
            int? maxExecutionTime = null)
        {
            this.queryText = queryText;
            this.queryParameters = queryParameters;
            this.resultMode = resultMode;
            jsonContractResolver = contractResolver ?? GraphClient.DefaultJsonContractResolver;
            this.maxExecutionTime = maxExecutionTime;
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

        public IContractResolver JsonContractResolver
        {
            get { return jsonContractResolver; }
        }

        public int? MaxExecutionTime
        {
            get { return maxExecutionTime; }
        }

        CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer { JsonConverters = GraphClient.DefaultJsonConverters, JsonContractResolver = jsonContractResolver };
        }

        public string DebugQueryText
        {
            get
            {
                var serializer = BuildSerializer();
                var text = queryParameters
                    .Keys
                    .Aggregate(
                        queryText,
                        (current, paramName) =>
                        {
                            var value = queryParameters[paramName];
                            value = serializer.Serialize(value);
                            return current.Replace("{" + paramName + "}", value.ToString());
                        });

                return text;
            }
        }
    }
}
