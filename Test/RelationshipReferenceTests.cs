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
    }
}