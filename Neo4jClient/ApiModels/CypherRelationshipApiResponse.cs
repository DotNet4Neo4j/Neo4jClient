using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    class CypherRelationshipApiResponse<TData>
                where TData : class, new()
    {
        [JsonProperty("data")]
        public List<List<RelationshipApiResponse<TData>>> Data { get; set; }

        [JsonProperty("columns")]
        public List<string> Columns { get; set; }
    }
}