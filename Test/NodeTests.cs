using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void ClientShouldReturnClientFromReference()
        {
            var client = Substitute.For<IGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            Assert.AreEqual(client, ((IGremlinQuery)node).Client);
        }

        [Test]
        public void QueryTextShouldReturnSimpleVectorStep()
        {
            var client = Substitute.For<IGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            Assert.AreEqual("g.v(123)", ((IGremlinQuery)node).QueryText);
        }
    }
}