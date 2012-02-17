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
        public void GremlinQueryShouldReturnSimpleVectorStep()
        {
            var client = Substitute.For<IGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            var query = (IGremlinQuery)node;
            Assert.AreEqual("g.v(p0)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void CypherQueryShouldIncludeNodeAsStartBit()
        {
            var client = Substitute.For<IGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            var query = node.StartCypher("foo").Query;
            Assert.AreEqual("START foo=node({p0})", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void CypherQueryShouldIncludeRootNodeAsStartBit()
        {
            var client = Substitute.For<IGraphClient>();
            client.RootNode.ReturnsForAnyArgs(new NodeReference(0, client));
            var query = client.RootNode.StartCypher("foo").Query;
            Assert.AreEqual("START foo=node({p0})", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters["p0"]);
        }

        [Test]
        public void CypherQueryShouldPreserveClientReference()
        {
            var client = Substitute.For<IGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            var queryBuilder = (IAttachedReference)node.StartCypher("foo");
            Assert.AreEqual(client, queryBuilder.Client);
        }

        [Test]
        public void EqualityOperatorShouldReturnTrueForEquivalentReferences()
        {
            var node1 = new Node<object>(new object(), new NodeReference<object>(123));
            var node2 = new Node<object>(new object(), new NodeReference<object>(123));
            Assert.IsTrue(node1 == node2);
        }

        [Test]
        public void EqualityOperatorShouldReturnFalseForDifferentReferences()
        {
            var node1 = new Node<object>(new object(), new NodeReference<object>(123));
            var node2 = new Node<object>(new object(), new NodeReference<object>(456));
            Assert.IsFalse(node1 == node2);
        }

        [Test]
        public void GetHashCodeShouldReturnNodeId()
        {
            var node = new Node<object>(new object(), new NodeReference<object>(123));
            Assert.AreEqual(123, node.Reference.Id);
        }
    }
}