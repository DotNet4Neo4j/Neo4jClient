using System;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests
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
        public void EqualsTest(int lhs, int rhs, bool expected)
        {
            (new NodeReference(lhs) == new NodeReference(rhs)).Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 2, false)]
        [InlineData(3, 3, true)]
        public void GetHashCodeTest(int lhs, int rhs, bool expected)
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
    }
}