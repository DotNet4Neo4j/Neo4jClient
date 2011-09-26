using System.Diagnostics;
using Newtonsoft.Json;
using RestSharp;

namespace Neo4jClient.ApiModels
{
    [DebuggerDisplay("{Id}: {Method} {To}")]
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
