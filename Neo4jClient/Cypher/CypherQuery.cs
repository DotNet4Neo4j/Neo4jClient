using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Serialization;

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
        readonly CypherResultFormat resultFormat;
		readonly IContractResolver jsonContractResolver;

        public CypherQuery(
            string queryText,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode,
			IContractResolver contractResolver = null) :
            this(queryText, queryParameters, resultMode, CypherResultFormat.DependsOnEnvironment, contractResolver)
        {
        }

        public CypherQuery(
            string queryText,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode,
            CypherResultFormat resultFormat,
			IContractResolver contractResolver = null)
        {
            this.queryText = queryText;
            this.queryParameters = queryParameters;
            this.resultMode = resultMode;
            this.resultFormat = resultFormat;
            jsonContractResolver = contractResolver ?? GraphClient.DefaultJsonContractResolver;
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

        public IContractResolver JsonContractResolver
        {
            get { return jsonContractResolver; }
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
