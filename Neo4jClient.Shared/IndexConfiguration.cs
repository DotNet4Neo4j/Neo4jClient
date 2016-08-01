using Newtonsoft.Json;

namespace Neo4jClient
{
    public class IndexConfiguration
    {
        [JsonProperty("type")]
        public IndexType Type { get; set; }
        [JsonProperty("provider")]
        public IndexProvider Provider { get; set; }
    }
}