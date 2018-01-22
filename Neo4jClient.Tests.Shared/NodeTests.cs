using NSubstitute;
using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test
{
    
    public class NodeTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ClientShouldReturnClientFromReference()
        {
            var client = Substitute.For<IGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            Assert.Equal(client, ((IGremlinQuery)node).Client);
        }

        [Fact]
        public void GremlinQueryShouldReturnSimpleVectorStep()
        {
            var client = Substitute.For<IGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            var query = (IGremlinQuery)node;
            Assert.Equal("g.v(p0)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void CypherQueryShouldIncludeNodeAsStartBit()
        {
            var client = Substitute.For<IRawGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            var query = node.StartCypher("foo").Query;
            Assert.Equal("START foo=node({p0})", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void CypherQueryShouldIncludeRootNodeAsStartBit()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.RootNode.ReturnsForAnyArgs(new RootNode(4, client));
            var query = client.RootNode.StartCypher("foo").Query;
            Assert.Equal("START foo=node({p0})", query.QueryText);
            Assert.Equal(4L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void CypherQueryShouldPreserveClientReference()
        {
            var client = Substitute.For<IRawGraphClient>();
            var reference = new NodeReference<object>(123, client);
            var node = new Node<object>(new object(), reference);
            var queryBuilder = (IAttachedReference)node.StartCypher("foo");
            Assert.Equal(client, queryBuilder.Client);
        }

        [Fact]
        public void EqualityOperatorShouldReturnTrueForEquivalentReferences()
        {
            var node1 = new Node<object>(new object(), new NodeReference<object>(123));
            var node2 = new Node<object>(new object(), new NodeReference<object>(123));
            Assert.True(node1 == node2);
        }

        [Fact]
        public void EqualityOperatorShouldReturnFalseForDifferentReferences()
        {
            var node1 = new Node<object>(new object(), new NodeReference<object>(123));
            var node2 = new Node<object>(new object(), new NodeReference<object>(456));
            Assert.False(node1 == node2);
        }

        [Fact]
        public void GetHashCodeShouldReturnNodeId()
        {
            var node = new Node<object>(new object(), new NodeReference<object>(123));
            Assert.Equal(123, node.Reference.Id);
        }
    }
}