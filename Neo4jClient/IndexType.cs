using Newtonsoft.Json;

namespace Neo4jClient
{
    public enum IndexType
    {
        [JsonProperty("fulltext")]
        FullText,

        [JsonProperty("exact")]
        Exact
    }
}