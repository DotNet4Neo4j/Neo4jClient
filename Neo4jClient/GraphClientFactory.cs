using System;
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

        public IGraphClient Create()
        {
            var client = new GraphClient(
                _configuration.RootUri,
                _configuration.Username,
                _configuration.Password);

            client.Connect(_configuration);

            return client;
        }

        public IGraphClient Create(IHttpClient httpClient)
        {
            var client = new GraphClient(_configuration.RootUri, httpClient);

            client.Connect(_configuration);

            return client;
        }
    }
}
