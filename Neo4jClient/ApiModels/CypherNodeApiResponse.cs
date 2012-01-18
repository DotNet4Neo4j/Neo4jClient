using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    class CypherNodeApiResponse<TNode>
    {
        [JsonProperty("data")]
        public List<List<NodeApiResponse<TNode>>>  Data { get; set; }

        [JsonProperty("columns")]
        public List<string> Columns { get; set; }
    }
}