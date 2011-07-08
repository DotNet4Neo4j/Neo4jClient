using NUnit.Framework;
using Neo4jClient.Gremlin;

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

        [Test]
        public void ShouldAllowDirectCreationOfTypedReference()
        {
            var nodeReference = new NodeReference<object>(3);
            Assert.AreEqual(3, nodeReference.Id);
        }

        [Test]
        [TestCase(1, 2, Result = false)]
        [TestCase(3, 3, Result = true)]
        public bool Equals(int lhs, int rhs)
        {
            return new NodeReference(lhs) == new NodeReference(rhs);
        }

        [Test]
        [TestCase(1, 2, Result = false)]
        [TestCase(3, 3, Result = true)]
        public bool GetHashCode(int lhs, int rhs)
        {
            return new NodeReference(lhs).GetHashCode() == new NodeReference(rhs).GetHashCode();
        }

        [Test]
        public void EqualsOperatorShouldReturnFalseWhenComparingInstanceWithNull()
        {
            var lhs = new NodeReference(3);
            Assert.IsFalse(lhs == null);
        }

        [Test]
        public void EqualsOperatorShouldReturnTrueWhenComparingNullWithNull()
        {
            NodeReference lhs = null;
            Assert.IsTrue(lhs == null);
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingWithNull()
        {
            var lhs = new NodeReference(3);
            Assert.IsFalse(lhs.Equals(null));
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingWithDifferentType()
        {
            var lhs = new NodeReference(3);
            Assert.IsFalse(lhs.Equals(new object()));
        }

        [Test]
        public void EqualsShouldReturnTrueWhenComparingRootNodeWithZero()
        {
            var lhs = NodeReference.RootNode;
            var rhs = new NodeReference(0);
            Assert.IsTrue(lhs == rhs);
        }

        [Test]
        public void EqualsShouldReturnTrueWhenComparingRootNodeWithRootNode()
        {
            var lhs = NodeReference.RootNode;
            var rhs = NodeReference.RootNode;
            Assert.IsTrue(lhs == rhs);
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingRootNodeWithNonZero()
        {
            var lhs = NodeReference.RootNode;
            var rhs = new NodeReference(4);
            Assert.IsFalse(lhs == rhs);
        }

        [Test]
        public void NodeTypeShouldReturnTypedNodeType()
        {
            var reference = (NodeReference)new NodeReference<Randomizer>(123);
            Assert.AreEqual(typeof(Randomizer), reference.NodeType);
        }

        [Test]
        public void NodeTypeShouldReturnNullWhenUntyped()
        {
            var reference = new NodeReference(123);
            Assert.IsNull(reference.NodeType);
        }

        [Test]
        public void GremlinQueryTextShouldReturnSimpleVectorStep()
        {
            var reference = new NodeReference(123);
            var queryText = ((IGremlinQuery) reference).QueryText;
            Assert.AreEqual("g.v(123)", queryText);
        }
    }
}