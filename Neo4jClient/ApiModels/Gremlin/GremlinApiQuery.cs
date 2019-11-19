using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Gremlin
{
    class GremlinApiQuery
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
        public IDictionary<string, object> Parameters
        {
            get { return parameters; }
        }
    }
}
