using System.Net;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    class BatchStepResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
