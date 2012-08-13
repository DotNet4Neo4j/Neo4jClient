using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class RootNodeTests
    {
        [Test]
        public void RootNodeShouldHaveReferenceBackToClient()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectGraphClient();
                var rootNode = client.RootNode;
                Assert.AreEqual(client, ((IGremlinQuery) rootNode).Client);
            }
        }

        [Test]
        public void RootNodeShouldSupportGremlinQueries()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectGraphClient();
                var rootNode = client.RootNode;
                Assert.AreEqual("g.v(p0)", ((IGremlinQuery) rootNode).QueryText);
                Assert.AreEqual(123, ((IGremlinQuery) rootNode).QueryParameters["p0"]);
            }
        }
    }
}
