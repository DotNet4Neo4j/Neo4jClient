using System;

namespace Neo4jClient
{
    public class GraphClientFactory : IGraphClientFactory
    {
        private readonly NeoServerConfiguration _configuration;

        public GraphClientFactory(NeoServerConfiguration configuration)
        {
            if (_configuration == null)
             throw new ArgumentNullException("configuration", "Neo server configuration is null");

            if (configuration.ApiConfig == null)
                throw new ArgumentException("Root Api configuration is not defined", "ApiConfig");

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
    }
}
