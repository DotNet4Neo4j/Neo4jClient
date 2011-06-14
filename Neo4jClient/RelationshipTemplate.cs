using Newtonsoft.Json;

namespace Neo4jClient
{
    internal class RelationshipTemplate
    {
        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}