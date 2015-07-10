using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Serialization
{
    public class CustomJsonSerializer : ISerializer
    {
        public IEnumerable<JsonConverter> JsonConverters { get; set; }
        public string ContentType { get; set; }
        public string DateFormat { get; set; }
        public string Namespace { get; set; }
        public string RootElement { get; set; }
        public NullValueHandling NullHandling {get; set;}
        public bool QuoteName { get; set; }
        public IContractResolver JsonContractResolver { get; set; }

        public CustomJsonSerializer()
        {
            ContentType = "application/json";
            NullHandling = NullValueHandling.Ignore;
            QuoteName = true;
            JsonContractResolver = GraphClient.DefaultJsonContractResolver;
        }

        public string Serialize(object obj)
        {
            var serializer = new JsonSerializer
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullHandling,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = JsonContractResolver
            };

            if (JsonConverters != null)
            {
                foreach (var converter in JsonConverters.Reverse())
                    serializer.Converters.Add(converter);
            }

            using (var stringWriter = new StringWriter())
            using (var jsonTextWriter = new JsonTextWriter(stringWriter) { QuoteName = QuoteName })
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                jsonTextWriter.QuoteChar = '"';
                serializer.Serialize(jsonTextWriter, obj);
                return stringWriter.ToString();
            }
        }
        public T Deserialize<T>(string content)
        {
            var serializer = new JsonSerializer
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullHandling,
                DefaultValueHandling = DefaultValueHandling.Include
            };

            if (JsonConverters != null)
            {
                foreach (var converter in JsonConverters.Reverse())
                    serializer.Converters.Add(converter);
            }

            using (var reader = new StringReader(content))
            using (var jsonReader = new JsonTextReader(reader))
                return serializer.Deserialize<T>(jsonReader);
        }
    }
}