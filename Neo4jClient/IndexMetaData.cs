using Newtonsoft.Json;

namespace Neo4jClient
{
    public class IndexMetaData
    {
        [JsonProperty("to_lower_case")]
        public bool ToLowerCase { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}