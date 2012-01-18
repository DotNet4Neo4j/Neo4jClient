using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    class CypherApiQuery
    {
        readonly string query;
        readonly IDictionary<string, object> parameters;

        public CypherApiQuery(string query, IDictionary<string, object> parameters)
        {
            this.query = query;
            this.parameters = parameters ?? new Dictionary<string, object>();
        }

        [JsonProperty("query")]
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
