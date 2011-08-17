using Newtonsoft.Json;

namespace Neo4jClient
{
    internal class CreateIndexApiRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("config")]
        public IndexConfiguration Configuration{ get; set; }
    }
}