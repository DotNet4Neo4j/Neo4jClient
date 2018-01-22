using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class ServerVersionTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldParse15M02Version()
        {
            using (var testHarness = new RestTestHarness
            {
                { MockRequest.Get(""), MockResponse.NeoRoot() }
            })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                Assert.Equal("1.5.0.2", graphClient.ServerVersion.ToString());
            }
        }
    }
}