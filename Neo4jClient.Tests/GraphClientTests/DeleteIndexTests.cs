using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class DeleteIndexTests
    {
        [Test]
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
