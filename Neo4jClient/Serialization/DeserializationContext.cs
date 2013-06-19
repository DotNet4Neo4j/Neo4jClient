using System.Globalization;
using Newtonsoft.Json;

namespace Neo4jClient.Serialization
{
    public class DeserializationContext
    {
        public CultureInfo Culture { get; set; }
        public JsonConverter[] JsonConverters { get; set; }
    }
}
