using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class DeleteIndexTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldExecuteSilentlyForSuccessfulDelete()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Delete("/index/node/MyIndex"),
                    MockResponse.Http(204)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                graphClient.DeleteIndex("MyIndex", IndexFor.Node);
            }
        }
    }
}
