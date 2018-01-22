using System;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test
{
    
    public class NodeReferenceTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldImplicitlyCastFromIntToUntypedReference()
        {
            NodeReference nodeReference = 3;
            Assert.Equal(3, nodeReference.Id);
        }

        [Fact]
        public void ShouldImplicitlyCastFromIntToTypedReference()
        {
            NodeReference<object> nodeReference = 3;
            Assert.Equal(3, nodeReference.Id);
        }

        [Fact]
        public void ShouldExplicitlyCastFromIntToUntypedReference()
        {
            var nodeReference = (NodeReference)3;
            Assert.IsAssignableFrom(typeof(NodeReference), nodeReference);
            Assert.Equal(3, nodeReference.Id);
        }

        [Fact]
        public void ShouldExplicitlyCastFromIntToTypedReference()
        {
            var nodeReference = (NodeReference<object>)3;
            Assert.IsAssignableFrom(typeof(NodeReference<object>), nodeReference);
            Assert.Equal(3, nodeReference.Id);
        }

        [Fact]
        public void ShouldAllowDirectCreationOfTypedReference()
        {
            var nodeReference = new NodeReference<object>(3);
            Assert.Equal(3, nodeReference.Id);
        }

        [Theory]
        [InlineData(1, 2, false)]
        [InlineData(3, 3, true)]
        public void Equals(int lhs, int rhs, bool expected)
        {
            (new NodeReference(lhs) == new NodeReference(rhs)).Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 2, false)]
        [InlineData(3, 3, true)]
        public void GetHashCode(int lhs, int rhs, bool expected)
        {
            (new NodeReference(lhs).GetHashCode() == new NodeReference(rhs).GetHashCode()).Should().Be(expected);
        }

        [Fact]
        public void EqualsOperatorShouldReturnFalseWhenComparingInstanceWithNull()
        {
            var lhs = new NodeReference(3);
            Assert.False(lhs == null);
        }

        [Fact]
        public void EqualsOperatorShouldReturnTrueWhenComparingNullWithNull()
        {
            NodeReference lhs = null;
            Assert.True(lhs == null);
        }

        [Fact]
        public void EqualsShouldReturnFalseWhenComparingWithNull()
        {
            var lhs = new NodeReference(3);
            Assert.False(lhs.Equals(null));
        }

        [Fact]
        public void EqualsShouldReturnFalseWhenComparingWithDifferentType()
        {
            var lhs = new NodeReference(3);
            Assert.False(lhs.Equals(new object()));
        }

        [Fact]
        public void EqualsShouldReturnTrueWhenComparingRootNodeWithNodeReferenceOfSameId()
        {
            var lhs = new RootNode(123);
            var rhs = new NodeReference(123);
            Assert.True(lhs == rhs);
        }

        [Fact]
        public void EqualsShouldReturnTrueWhenComparingRootNodeWithRootNodeOfSameId()
        {
            var lhs = new RootNode(123);
            var rhs = new RootNode(123);
            Assert.True(lhs == rhs);
        }

        [Fact]
        public void EqualsShouldReturnFalseWhenComparingRootNodeWithRootNodeOfDifferentId()
        {
            var lhs = new RootNode(123);
            var rhs = new RootNode(456);
            Assert.False(lhs == rhs);
        }

        [Fact]
        public void EqualsShouldReturnFalseWhenComparingRootNodeWithNodeReferenceOfDifferentId()
        {
            var lhs = new RootNode(123);
            var rhs = new NodeReference(4);
            Assert.False(lhs == rhs);
        }

        [Fact]
        public void NodeTypeShouldReturnTypedNodeType()
        {
            var reference = (NodeReference)new NodeReference<FactAttribute>(123);
            Assert.Equal(typeof(FactAttribute), reference.NodeType);
        }

        [Fact]
        public void NodeTypeShouldReturnNullWhenUntyped()
        {
            var reference = new NodeReference(123);
            Assert.Null(reference.NodeType);
        }

        [Fact]
        public void TypedNodeReferenceShouldThrowExceptionIfTNodeIsWrappedAgain()
        {
// ReSharper disable ObjectCreationAsStatement
            var ex = Assert.Throws<NotSupportedException>(() => new NodeReference<Node<TimeZone>>(123));
            Assert.Equal("You're trying to initialize NodeReference<Node<System.TimeZone>> which is too many levels of nesting. You should just be using NodeReference<System.TimeZone> instead. (You use a Node, or a NodeReference, but not both together.)", ex.Message);
// ReSharper restore ObjectCreationAsStatement
        }

        [Fact]
        public void GremlinQueryTextShouldReturnSimpleVectorStep()
        {
            var reference = new NodeReference(123);
            var query = ((IGremlinQuery) reference);
            Assert.Equal("g.v(p0)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void CypherShouldStartQueryFromCurrentNodeReference()
        {
            var graphClient = Substitute.For<IRawGraphClient>();
            var reference = new NodeReference(123, graphClient);
            var query = reference.StartCypher("foo").Query;
            Assert.Equal("START foo=node({p0})", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }
    }
}