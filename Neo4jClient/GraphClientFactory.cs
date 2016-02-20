namespace Neo4jClient
{
    public class GraphClientFactory : IGraphClientFactory
    {
        private readonly NeoServerConfiguration _configuration;

        public GraphClientFactory(NeoServerConfiguration configuration)
        {
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
