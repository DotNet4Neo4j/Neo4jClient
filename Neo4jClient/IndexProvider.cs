using Newtonsoft.Json;

namespace Neo4jClient
{
    public enum IndexProvider
    {
        [JsonProperty("lucene")]
        Lucene
    }
}