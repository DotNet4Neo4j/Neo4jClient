using NUnit.Framework;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class NodeReferenceTests
    {
        [Test]
        public void ShouldImplicitlyCastFromInt()
        {
            NodeReference nodeReference = 3;
            Assert.AreEqual(3, nodeReference.Id);
        }

        [Test]
        public void ShouldExplicitlyCastFromInt()
        {
            var nodeReference = (NodeReference)3;
            Assert.IsInstanceOf(typeof(NodeReference), nodeReference);
            Assert.AreEqual(3, nodeReference.Id);
        }
    }
}