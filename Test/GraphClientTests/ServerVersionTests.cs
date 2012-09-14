using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ServerVersionTests
    {
        [Test]
        public void ShouldParse15M02Version()
        {
            using (var testHarness = new RestTestHarness
            {
                { MockRequest.Get(""), MockResponse.NeoRoot() }
            })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                Assert.AreEqual("1.5.0.2", graphClient.ServerVersion.ToString());
            }
        }
    }
}