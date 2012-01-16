using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    class GremlinPluginApiResponse
    {
        [JsonProperty("execute_script")]
        public string ExecuteScript { get; set; }
    }
}
