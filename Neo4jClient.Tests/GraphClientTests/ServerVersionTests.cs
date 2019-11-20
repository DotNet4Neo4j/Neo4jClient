using System.Threading.Tasks;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class ServerVersionTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task ShouldParse15M02Version()
        {
            using (var testHarness = new RestTestHarness
            {
                { MockRequest.Get(""), MockResponse.NeoRoot() }
            })
            {
                var graphClient = (GraphClient)await testHarness.CreateAndConnectGraphClient();

                Assert.Equal("1.5.0.2", graphClient.ServerVersion.ToString());
            }
        }
    }
}