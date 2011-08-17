using Newtonsoft.Json;

namespace Neo4jClient
{
    public internal enum IndexType
    {
        [JsonProperty("fulltext")]
        FullText,

        [JsonProperty("exact")]
        Exact
    }
}