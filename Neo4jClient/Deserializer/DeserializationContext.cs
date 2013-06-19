using System.Globalization;
using Newtonsoft.Json;

namespace Neo4jClient.Deserializer
{
    public class DeserializationContext
    {
        public CultureInfo Culture { get; set; }
        public JsonConverter[] JsonConverters { get; set; }
    }
}
