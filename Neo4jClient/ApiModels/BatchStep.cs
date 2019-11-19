using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    [DebuggerDisplay("{Id}: {Method} {To}")]
    class BatchStep
    {
        [JsonIgnore]
        public HttpMethod Method { get; set; }

        [JsonProperty("method")]
        public string MethodAsString
        {
            get { return Method.Method; }
        }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("body")]
        public object Body { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
