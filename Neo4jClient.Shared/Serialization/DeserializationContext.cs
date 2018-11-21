using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Serialization
{
    public class DeserializationContext<TTypeConverter>
    {
        public CultureInfo Culture { get; set; }
        public TTypeConverter[] Converters { get; set; }
        public IContractResolver JsonContractResolver { get; set; }
        public TypeMapping[] TypeMappings { get; set; }
    }
}
