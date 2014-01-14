using System.Net.Http;
using System.Threading.Tasks;

namespace Neo4jClient.Execution
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
