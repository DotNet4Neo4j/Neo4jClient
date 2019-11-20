using System;
using System.Threading.Tasks;
using Neo4jClient.Execution;

namespace Neo4jClient
{
    public class GraphClientFactory : IGraphClientFactory
    {
        private readonly NeoServerConfiguration _configuration;

        public GraphClientFactory(NeoServerConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration", "Neo server configuration is null");

            _configuration = configuration;
        }

        public async Task<IGraphClient> CreateAsync()
        {
            var client = new GraphClient(
                _configuration.RootUri,
                _configuration.Username,
                _configuration.Password);

            await client.ConnectAsync(_configuration).ConfigureAwait(false);

            return client;
        }

        public async Task<IGraphClient> CreateAsync(IHttpClient httpClient)
        {
            var client = new GraphClient(_configuration.RootUri, httpClient);

            await client.ConnectAsync(_configuration).ConfigureAwait(false);

            return client;
        }
    }
}
