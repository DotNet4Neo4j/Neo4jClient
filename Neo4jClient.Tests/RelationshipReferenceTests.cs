using FluentAssertions;
using Xunit;

namespace Neo4jClient.Tests
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
            Assert.IsAssignableFrom<RelationshipReference>(relationshipReference);
            Assert.Equal(3, relationshipReference.Id);
        }

        [Theory]
        [InlineData(1, 2, false)]
        [InlineData(3, 3, true)]
        public void EqualsTest(int lhs, int rhs, bool expected)
        {
            (new RelationshipReference(lhs) == new RelationshipReference(rhs)).Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 2, false)]
        [InlineData(3, 3, true)]
        public void GetHashCodeTest(int lhs, int rhs, bool expected)
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
    }
}