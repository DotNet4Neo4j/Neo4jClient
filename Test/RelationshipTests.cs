using System;
using System.Linq;
using NUnit.Framework;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class RelationshipTests
    {
        [Test]
        public void GetAllowedSourceNodeTypesShouldReturnAllTypes()
        {
            // Act
            var types = Relationship.GetAllowedNodeTypes(typeof (TestRelationship), RelationshipEnd.SourceNode);

            // Assert
            CollectionAssert.AreEquivalent(
                new[] { typeof(Foo), typeof(Bar) },
                types.ToArray()
            );
        }

        [Test]
        public void GetAllowedTargetNodeTypesShouldReturnAllTypes()
        {
            // Act
            var types = Relationship.GetAllowedNodeTypes(typeof(TestRelationship), RelationshipEnd.TargetNode);

            // Assert
            CollectionAssert.AreEquivalent(
                new[] { typeof(Bar), typeof(Baz) },
                types.ToArray()
            );
        }

        [Test]
        [TestCase(RelationshipDirection.Incoming)]
        [TestCase(RelationshipDirection.Outgoing)]
        public void DetermineRelationshipDirectionShouldReturnExplicitDirection(RelationshipDirection direction)
        {
            // Arrange
            var relationship = new TestRelationship(new NodeReference(0)) { Direction = direction };
            var calculatedDirection = Relationship.DetermineRelationshipDirection(null, relationship);
            Assert.AreEqual(direction, calculatedDirection);
        }

        [Test]
        public void DetermineRelationshipDirectionShouldReturnOutgoingWhenBaseNodeIsOnlyValidAsASourceNodeAndOtherNodeIsOnlyValidAsATargetNode()
        {
            var baseNodeType = typeof(Foo);
            var relationship = new TestRelationship(new NodeReference<Baz>(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.AreEqual(RelationshipDirection.Outgoing, calculatedDirection);
        }

        [Test]
        public void DetermineRelationshipDirectionShouldReturnOutgoingWhenBaseNodeIsOnlyValidAsASourceNodeEvenIfOtherNodeIsAlsoValidAsASourceNode()
        {
            var baseNodeType = typeof(Foo);
            var relationship = new TestRelationship(new NodeReference<Bar>(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.AreEqual(RelationshipDirection.Outgoing, calculatedDirection);
        }

        [Test]
        public void DetermineRelationshipDirectionShouldReturnIncomingWhenBaseNodeIsOnlyValidAsATargetNodeAndOtherNodeIsOnlyValidAsASourceNode()
        {
            var baseNodeType = typeof(Baz);
            var relationship = new TestRelationship(new NodeReference<Foo>(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.AreEqual(RelationshipDirection.Incoming, calculatedDirection);
        }

        [Test]
        public void DetermineRelationshipDirectionShouldReturnIncomingWhenBaseNodeIsOnlyValidAsATargetNodeEvenIfOtherNodeIsAlsoValidAsATargetNode()
        {
            var baseNodeType = typeof(Baz);
            var relationship = new TestRelationship(new NodeReference<Bar>(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.AreEqual(RelationshipDirection.Incoming, calculatedDirection);
        }

        [Test]
        [ExpectedException(typeof(AmbiguousRelationshipDirectionException))]
        public void DetermineRelationshipDirectionShouldThrowExceptionWhenBothNodesAreValidAsSourceAndTargetNodes()
        {
            var baseNodeType = typeof(Bar);
            var relationship = new TestRelationship(new NodeReference<Bar>(123));
            Relationship.DetermineRelationshipDirection(baseNodeType, relationship);
        }

        [Test]
        [ExpectedException(typeof(AmbiguousRelationshipDirectionException))]
        public void DetermineRelationshipDirectionShouldReturnIncomingWhenBaseNodeOnlyValidAsTargetAndSourceNodeNotDefinedAsEither()
        {
            var baseNodeType = typeof(Baz);
            var relationship = new TestRelationship(new NodeReference<Qak>(123));
            Assert.AreEqual(RelationshipDirection.Incoming, Relationship.DetermineRelationshipDirection(baseNodeType, relationship));
        }

        [Test]
        [ExpectedException(typeof(AmbiguousRelationshipDirectionException))]
        public void DetermineRelationshipDirectionShouldThrowExceptionWhenNeitherNodeIsValidAtEitherEnd()
        {
            var baseNodeType = typeof(Zip);
            var relationship = new TestRelationship(new NodeReference<Qak>(123));
            Relationship.DetermineRelationshipDirection(baseNodeType, relationship);
        }

        [Test]
        public void DetermineRelationshipDirectionShouldReturnOutgoingWhenBaseNodeIsValidAsASourceNodeAndOtherNodeIsAnUntypedReference()
        {
            var baseNodeType = typeof(Bar);
            var relationship = new TestRelationship(new NodeReference(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.AreEqual(RelationshipDirection.Outgoing, calculatedDirection);
        }

        public class Foo { }
        public class Bar { }
        public class Baz { }
        public class Qak { }
        public class Zip { }

        public class TestRelationship : Relationship,
            IRelationshipAllowingSourceNode<Foo>,
            IRelationshipAllowingSourceNode<Bar>,
            IRelationshipAllowingTargetNode<Bar>,
            IRelationshipAllowingTargetNode<Baz>
        {
            public TestRelationship(NodeReference targetNode) : base(targetNode)
            {
            }

            public override string RelationshipTypeKey
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}