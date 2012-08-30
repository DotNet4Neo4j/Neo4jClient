using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Neo4jClient
{
    internal static class HttpContentExtensions
    {
        public static Task<T> ReadAsJson<T>(this HttpContent content, JsonSerializer serializer)
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
