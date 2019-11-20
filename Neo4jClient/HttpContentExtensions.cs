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
        public static Task<string> ReadAsStringAsync(this HttpContent content)
        {
            return content.ReadAsStringAsync();
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content, IEnumerable<JsonConverter> jsonConverters, DefaultContractResolver resolver)
            where T : new()
        {
            var stringContent = await content.ReadAsStringAsync().ConfigureAwait(false);
            return new CustomJsonDeserializer(jsonConverters, resolver:resolver).Deserialize<T>(stringContent);
        }

        public static Task<T> ReadAsJsonAsync<T>(this HttpContent content, IEnumerable<JsonConverter> jsonConverters) where T : new()
        {
            return content.ReadAsJsonAsync<T>(jsonConverters, null);
        }
    }
}
