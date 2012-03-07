using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.ApiModels.CustomPlugin;

namespace Neo4jClient.ApiModels
{
    class ExtensionsApiResponse
    {
        public GremlinPluginApiResponse GremlinPlugin { get; set; }
        public CustomNeo4JPluginApiResponse CustomNeo4JPlugin { get; set; }
    }
}
