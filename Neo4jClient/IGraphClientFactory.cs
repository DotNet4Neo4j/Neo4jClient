using System.Threading.Tasks;
using Neo4jClient.Execution;

namespace Neo4jClient
{
    public interface IGraphClientFactory
    {
        Task<IGraphClient> CreateAsync();
        Task<IGraphClient> CreateAsync(IHttpClient client);
    }
}
