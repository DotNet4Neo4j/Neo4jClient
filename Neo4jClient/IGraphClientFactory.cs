using Neo4jClient.Execution;

namespace Neo4jClient
{
    public interface IGraphClientFactory
    {
        IGraphClient Create();
        IGraphClient Create(IHttpClient client);
    }
}
