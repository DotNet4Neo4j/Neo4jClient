using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient
{
    internal class GremlinApiQuery
    {
        readonly string query;
        readonly IDictionary<string, object> parameters;

        public GremlinApiQuery(string query, IDictionary<string, object> parameters)
        {
            this.query = query;
            this.parameters = parameters ?? new Dictionary<string, object>();
        }

        [JsonProperty("script")]
        public string Query
        {
            get { return query; }
        }

        [JsonProperty("params")]
        public string Parameters
        {
            // A nasty hack to workaround https://github.com/neo4j/community/issues/29
            get { return new RestSharp.Serializers.JsonSerializer().Serialize(parameters); }
        }
    }
}
