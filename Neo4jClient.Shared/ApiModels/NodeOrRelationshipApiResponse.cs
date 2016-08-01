using Newtonsoft.Json;

namespace Neo4jClient.ApiModels
{
    internal class NodeOrRelationshipApiResponse<TNode>
    {
        [JsonProperty("data")]
        public TNode Data { get; set; }
    }
}
