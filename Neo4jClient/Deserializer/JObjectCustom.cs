using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.Deserializer
{
    public class JObjectCustom : JObject
    {
        public static new JObject Parse(string json)
        {
            var reader = (JsonReader)new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = DateParseHandling.DateTimeOffset;
            var jobject = Load(reader);
            if (reader.Read() && reader.TokenType != JsonToken.Comment)
                JObject.Parse(json);
            return jobject;
        }
    }
}