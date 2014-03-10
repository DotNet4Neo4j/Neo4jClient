using System.Net.Http;
using System.Threading.Tasks;
using Neo4jClient.Execution;

namespace Neo4jClient
{
    public class HttpClientWrapper : IHttpClient
    {
        readonly HttpClient client;

        public HttpClientWrapper() : this(new HttpClient()) {}

        public HttpClientWrapper(HttpClient client)
        {
            this.client = client;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return client.SendAsync(request);
        }
    }
}
