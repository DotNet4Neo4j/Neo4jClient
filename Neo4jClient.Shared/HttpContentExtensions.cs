using System.Collections.Generic;
using System.Net.Http;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient
{
    internal static class HttpContentExtensions
    {
        public static string ReadAsString(this HttpContent content)
        {
            var readTask = content.ReadAsStringAsync();
            readTask.Wait();
            return readTask.Result;
        }

        public static T ReadAsJson<T>(this HttpContent content, IEnumerable<JsonConverter> jsonConverters, DefaultContractResolver resolver)
            where T : new()
        {
            var stringContent = content.ReadAsString();
            return new CustomJsonDeserializer(jsonConverters, resolver:resolver).Deserialize<T>(stringContent);
        }

        public static T ReadAsJson<T>(this HttpContent content, IEnumerable<JsonConverter> jsonConverters) where T : new()
        {
            return content.ReadAsJson<T>(jsonConverters, null);
        }
    }
}
