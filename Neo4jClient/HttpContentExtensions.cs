using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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

        public static Task<T> ReadAsJsonAsync<T>(this HttpContent content, IEnumerable<JsonConverter> jsonConverters, DefaultContractResolver resolver)
            where T : new()
        {
            return content.ReadAsStringAsync().Then<string, T>(stringContent => 
                new CustomJsonDeserializer(jsonConverters, resolver: resolver).Deserialize<T>(stringContent));
        }

        public static T ReadAsJson<T>(this HttpContent content, IEnumerable<JsonConverter> jsonConverters) where T : new()
        {
            return content.ReadAsJson<T>(jsonConverters, null);
        }

        public static Task<T> ReadAsJsonAsync<T>(this HttpContent content, IEnumerable<JsonConverter> jsonConverters) where T : new()
        {
            return content.ReadAsJsonAsync<T>(jsonConverters, null);
        }
    }
}
