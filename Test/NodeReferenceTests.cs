using System;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class NodeReferenceTests
    {
        [Test]
        public void ShouldImplicitlyCastFromIntToUntypedReference()
        {
            NodeReference nodeReference = 3;
            Assert.AreEqual(3, nodeReference.Id);
        }

        [Test]
        public void ShouldImplicitlyCastFromIntToTypedReference()
        {
            NodeReference<object> nodeReference = 3;
            Assert.AreEqual(3, nodeReference.Id);
        }

        [Test]
        public void ShouldExplicitlyCastFromIntToUntypedReference()
        {
            var nodeReference = (NodeReference)3;
            Assert.IsInstanceOf(typeof(NodeReference), nodeReference);
            Assert.AreEqual(3, nodeReference.Id);
        }

        [Test]
        public void ShouldExplicitlyCastFromIntToTypedReference()
        {
            var nodeReference = (NodeReference<object>)3;
            Assert.IsInstanceOf(typeof(NodeReference<object>), nodeReference);
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
        public void EqualsShouldReturnTrueWhenComparingRootNodeWithNodeReferenceOfSameId()
        {
            var lhs = new RootNode(123);
            var rhs = new NodeReference(123);
            Assert.IsTrue(lhs == rhs);
        }

        [Test]
        public void EqualsShouldReturnTrueWhenComparingRootNodeWithRootNodeOfSameId()
        {
            var lhs = new RootNode(123);
            var rhs = new RootNode(123);
            Assert.IsTrue(lhs == rhs);
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingRootNodeWithRootNodeOfDifferentId()
        {
            var lhs = new RootNode(123);
            var rhs = new RootNode(456);
            Assert.IsFalse(lhs == rhs);
        }

        [Test]
        public void EqualsShouldReturnFalseWhenComparingRootNodeWithNodeReferenceOfDifferentId()
        {
            var lhs = new RootNode(123);
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
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "You're tring to initialize NodeReference<Node<System.TimeZone>> which is too many levels of nesting. You should just be using NodeReference<System.TimeZone> instead. (You use a Node, or a NodeReference, but not both together.)")]
        public void TypedNodeReferenceShouldThrowExceptionIfTNodeIsWrappedAgain()
        {
// ReSharper disable ObjectCreationAsStatement
            new NodeReference<Node<TimeZone>>(123);
// ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void GremlinQueryTextShouldReturnSimpleVectorStep()
        {
            var reference = new NodeReference(123);
            var query = ((IGremlinQuery) reference);
            Assert.AreEqual("g.v(p0)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void CypherShouldStartQueryFromCurrentNodeReference()
        {
            var graphClient = Substitute.For<IRawGraphClient>();
            var reference = new NodeReference(123, graphClient);
            var query = reference.StartCypher("foo").Query;
            Assert.AreEqual("START foo=node({p0})", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }
    }
}