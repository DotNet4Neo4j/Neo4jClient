using FluentAssertions;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test
{
    
    public class RelationshipReferenceTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldImplicitlyCastFromInt()
        {
            RelationshipReference relationshipReference = 3;
            Assert.Equal(3, relationshipReference.Id);
        }

        [Fact]
        public void ShouldExplicitlyCastFromInt()
        {
            var relationshipReference = (RelationshipReference)3;
            Assert.IsAssignableFrom(typeof(RelationshipReference), relationshipReference);
            Assert.Equal(3, relationshipReference.Id);
        }

        [Theory]
        [InlineData(1, 2, false)]
        [InlineData(3, 3, true)]
        public void Equals(int lhs, int rhs, bool expected)
        {
            (new RelationshipReference(lhs) == new RelationshipReference(rhs)).Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 2, false)]
        [InlineData(3, 3, true)]
        public void GetHashCode(int lhs, int rhs, bool expected)
        {
            (new RelationshipReference(lhs).GetHashCode() == new RelationshipReference(rhs).GetHashCode()).Should().Be(expected);
        }

        [Fact]
        public void EqualsOperatorShouldReturnFalseWhenComparingInstanceWithNull()
        {
            var lhs = new RelationshipReference(3);
            Assert.False(lhs == null);
        }

        [Fact]
        public void EqualsOperatorShouldReturnTrueWhenComparingNullWithNull()
        {
            RelationshipReference lhs = null;
            Assert.True(lhs == null);
        }

        [Fact]
        public void EqualsShouldReturnFalseWhenComparingWithNull()
        {
            var lhs = new RelationshipReference(3);
            Assert.False(lhs.Equals(null));
        }

        [Fact]
        public void EqualsShouldReturnFalseWhenComparingWithDifferentType()
        {
            var lhs = new RelationshipReference(3);
            Assert.False(lhs.Equals(new object()));
        }

        [Fact]
        public void GremlinQueryTextShouldReturnSimpleEdgeStep()
        {
            var reference = new RelationshipReference(123);
            var query = ((IGremlinQuery)reference);
            Assert.Equal("g.e(p0)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }
    }
}