using Neo4jClient.Gremlin;
using NUnit.Framework;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class RelationshipReferenceTests
    {
        [Test]
        public void ShouldImplicitlyCastFromInt()
        {
            RelationshipReference relationshipReference = 3;
            Assert.AreEqual(3, relationshipReference.Id);
        }

        [Test]
        public void ShouldExplicitlyCastFromInt()
        {
            var relationshipReference = (RelationshipReference)3;
            Assert.IsInstanceOf(typeof(RelationshipReference), relationshipReference);
            Assert.AreEqual(3, relationshipReference.Id);
        }

        [Test]
        [TestCase(1, 2, Result = false)]
        [TestCase(3, 3, Result = true)]
        public bool Equals(int lhs, int rhs)
        {
            return new RelationshipReference(lhs) == new RelationshipReference(rhs);
        }

        [Test]
        [TestCase(1, 2, Result = false)]
        [TestCase(3, 3, Result = true)]
        public bool GetHashCode(int lhs, int rhs)
        {
            return new RelationshipReference(lhs).GetHashCode() == new RelationshipReference(rhs).GetHashCode();
        }

        [Test]
        public void EqualsOperatorShouldReturnFalseWhenComparingInstanceWithNull()
        {
            var lhs = new RelationshipReference(3);
            Assert.IsFalse(lhs == null);
        }

        [Test]
        public void EqualsOperatorShouldReturnTrueWhenComparingNullWithNull()
        {
            RelationshipReference lhs = null;
            Assert.IsTrue(lhs == null);
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingWithNull()
        {
            var lhs = new RelationshipReference(3);
            Assert.IsFalse(lhs.Equals(null));
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingWithDifferentType()
        {
            var lhs = new RelationshipReference(3);
            Assert.IsFalse(lhs.Equals(new object()));
        }

        [Test]
        public void GremlinQueryTextShouldReturnSimpleEdgeStep()
        {
            var reference = new RelationshipReference(123);
            var query = ((IGremlinQuery)reference);
            Assert.AreEqual("g.e(p0)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }
    }
}