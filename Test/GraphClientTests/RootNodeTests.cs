using System;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class RootNodeTests
    {
        [Test]
        public void RootNodeShouldHaveId0()
        {
            var client = new GraphClient(new Uri("http://foo"), null);
            var rootNode = client.RootNode;
            Assert.AreEqual(0, rootNode.Id);
        }

        [Test]
        public void RootNodeShouldHaveReferenceBackToClient()
        {
            var client = new GraphClient(new Uri("http://foo"), null);
            var rootNode = client.RootNode;
            Assert.AreEqual(client, ((IGremlinQuery)rootNode).Client);
        }
    }
}