using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    public class PathsResult
    {
        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("nodes")]
        public List<string> Nodes { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("relationships")]
        public List<string> Relationships { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }
    }
}
