using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Neo4jClient.Serialization
{
    public class CustomJsonDeserializer
    {
        readonly IEnumerable<JsonConverter> jsonConverters;
        readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public CustomJsonDeserializer(IEnumerable<JsonConverter> jsonConverters)
        {
            this.jsonConverters = jsonConverters;
        }

        public T Deserialize<T>(string content) where T : new()
        {
            var context = new DeserializationContext
                {
                    Culture = culture,
                    JsonConverters = (jsonConverters ?? new List<JsonConverter>(0)).Reverse().ToArray()
                };

            content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling = DateParseHandling.DateTimeOffset
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
