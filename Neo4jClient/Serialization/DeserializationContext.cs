using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Serialization
{
    public class DeserializationContext
    {
        public CultureInfo Culture { get; set; }
        public JsonConverter[] JsonConverters { get; set; }
        public IContractResolver JsonContractResolver { get; set; }
    }
}
