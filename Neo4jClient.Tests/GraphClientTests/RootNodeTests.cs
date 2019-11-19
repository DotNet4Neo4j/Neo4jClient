using Neo4jClient.Gremlin;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class RootNodeTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void RootNodeShouldHaveReferenceBackToClient()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectGraphClient();
                var rootNode = client.RootNode;
                Assert.Equal(client, ((IGremlinQuery) rootNode).Client);
            }
        }

        [Fact]
        public void RootNodeShouldSupportGremlinQueries()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectGraphClient();
                var rootNode = client.RootNode;
                Assert.Equal("g.v(p0)", ((IGremlinQuery) rootNode).QueryText);
                Assert.Equal(123L, ((IGremlinQuery) rootNode).QueryParameters["p0"]);
            }
        }
    }
}
