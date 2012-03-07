using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.CustomPlugin
{
    class CustomNeo4JPluginApiResponse
    {
        [JsonProperty("get_all_nodes")]
        public string GetAllNodes { get; set; }
    }
}
