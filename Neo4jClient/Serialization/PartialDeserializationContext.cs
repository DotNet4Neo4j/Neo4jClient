using Newtonsoft.Json.Linq;

namespace Neo4jClient.Serialization
{
    public class PartialDeserializationContext
    {
        public JToken RootResult { get; set; }
        public DeserializationContext DeserializationContext { get; set; }
    }
}