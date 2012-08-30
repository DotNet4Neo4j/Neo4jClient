using System.Net.Http;
using Neo4jClient.Deserializer;

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

        public static T ReadAsJson<T>(this HttpContent content) where T : new()
        {
            var stringContent = content.ReadAsString();
            return new CustomJsonDeserializer().Deserialize<T>(stringContent);
        }
    }
}
