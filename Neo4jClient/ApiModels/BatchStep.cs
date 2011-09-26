using Newtonsoft.Json;
using RestSharp;

namespace Neo4jClient
{
    internal class BatchStep
    {
        [JsonProperty("method")]
        public Method Method { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("body")]
        public object Body { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
