using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.Serialization.Json
{
    public class PartialDeserializationContext
    {
        public JToken RootResult { get; set; }
        public DeserializationContext<JsonConverter> DeserializationContext { get; set; }
    }
}