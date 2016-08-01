using System.Collections.Generic;
using System.Collections.Specialized;
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
        readonly int? maxExecutionTime;
        readonly NameValueCollection customHeaders;

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
            IContractResolver contractResolver = null, 
            int? maxExecutionTime = null, 
            NameValueCollection customHeaders = null
            )
        {
            this.queryText = queryText;
            this.queryParameters = queryParameters;
            this.resultMode = resultMode;
            this.resultFormat = resultFormat;
            jsonContractResolver = contractResolver ?? GraphClient.DefaultJsonContractResolver;
            this.maxExecutionTime = maxExecutionTime;
            this.customHeaders = customHeaders;
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

        public int? MaxExecutionTime
        {
            get { return maxExecutionTime; }
        }

        /// <summary>
        /// Custom headers to add to REST calls to Neo4j server.
        /// Example usage: This can be used to provide extra information to a Neo4j Loadbalancer. 
        /// </summary>
        public NameValueCollection CustomHeaders
        {
            get { return customHeaders;}
        }

        CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer { JsonConverters = GraphClient.DefaultJsonConverters, JsonContractResolver = jsonContractResolver };
        }

        public string DebugQueryText
        {
            get
            {
                if (queryParameters == null)
                {
                    return queryText;
                }

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
