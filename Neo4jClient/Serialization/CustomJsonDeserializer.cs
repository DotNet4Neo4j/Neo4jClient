using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Serialization
{
    public class CustomJsonDeserializer
    {
        readonly IEnumerable<JsonConverter> jsonConverters;
        readonly CultureInfo culture;
        readonly DefaultContractResolver jsonResolver;

        public CustomJsonDeserializer(IEnumerable<JsonConverter> jsonConverters) : this(jsonConverters, null)
        {
        }

        public CustomJsonDeserializer(IEnumerable<JsonConverter> jsonConverters, CultureInfo cultureInfo = null, DefaultContractResolver resolver = null)
        {
            this.jsonConverters = jsonConverters;
            culture = cultureInfo ?? CultureInfo.InvariantCulture;
            jsonResolver = resolver ?? GraphClient.DefaultJsonContractResolver;
        }

        public T Deserialize<T>(string content) where T : new()
        {
            var context = new DeserializationContext
                {
                    Culture = culture,
                    JsonConverters = (jsonConverters ?? new List<JsonConverter>(0)).Reverse().ToArray(),
                    JsonContractResolver = jsonResolver
                };

            content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling = DateParseHandling.None
            };

            var target = new T();

            if (target is IList)
            {
                var objType = target.GetType();
                var json = JToken.ReadFrom(reader);
                target = (T)CommonDeserializerMethods.BuildList(context, objType, json.Root.Children(), new TypeMapping[0], 0);
            }
            else if (target is IDictionary)
            {
                var root = JToken.ReadFrom(reader).Root;
                target = (T)CommonDeserializerMethods.BuildDictionary(context, target.GetType(), root.Children(), new TypeMapping[0], 0);
            }
            else
            {
                var root = JToken.ReadFrom(reader).Root;
                CommonDeserializerMethods.Map(context, target, root, new TypeMapping[0], 0);
            }

            return target;
        }
    }
}
