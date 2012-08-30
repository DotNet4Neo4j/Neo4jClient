using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        public static T ReadAsJson<T>(this HttpContent content, JsonSerializer serializer)
        {
            var readTask = content.ReadAsJsonAsync<T>(serializer);
            readTask.Wait();
            return readTask.Result;
        }

        public static Task<T> ReadAsJsonAsync<T>(this HttpContent content, JsonSerializer serializer)
        {
            return content
                .ReadAsStreamAsync()
                .ContinueWith(streamTask =>
                {
                    var stream = streamTask.Result;
                    using (stream)
                    using (var reader = new StreamReader(stream))
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        return serializer.Deserialize<T>(jsonReader);
                    }
                });
        }
    }
}
