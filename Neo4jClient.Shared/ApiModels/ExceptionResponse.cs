using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    internal class ExceptionResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("exception")]
        public string Exception { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("stacktrace")]
        public string[] StackTrace { get; set; }
    }
}
