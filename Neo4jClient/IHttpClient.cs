using System.Net.Http;
using System.Threading.Tasks;

namespace Neo4jClient
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
