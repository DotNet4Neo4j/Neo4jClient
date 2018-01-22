using System;
using System.Linq;
using FluentAssertions;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test
{
    
    public class RelationshipTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void GetAllowedSourceNodeTypesShouldReturnAllTypes()
        {
            // Act
            var types = Relationship.GetAllowedNodeTypes(typeof (TestRelationship), RelationshipEnd.SourceNode);

            // Assert
            new[] {typeof(Foo), typeof(Bar)}.Should().BeEquivalentTo(types.ToArray());
        }

        [Fact]
        public void GetAllowedTargetNodeTypesShouldReturnAllTypes()
        {
            // Act
            var types = Relationship.GetAllowedNodeTypes(typeof(TestRelationship), RelationshipEnd.TargetNode);

            new[] {typeof(Bar), typeof(Baz)}.Should().BeEquivalentTo(types.ToArray());
        }

        [Theory]
        [InlineData(RelationshipDirection.Incoming)]
        [InlineData(RelationshipDirection.Outgoing)]
        public void DetermineRelationshipDirectionShouldReturnExplicitDirection(RelationshipDirection direction)
        {
            // Arrange
            var relationship = new TestRelationship(new NodeReference(0)) { Direction = direction };
            var calculatedDirection = Relationship.DetermineRelationshipDirection(null, relationship);
            Assert.Equal(direction, calculatedDirection);
        }

        [Fact]
        public void DetermineRelationshipDirectionShouldReturnOutgoingWhenBaseNodeIsOnlyValidAsASourceNodeAndOtherNodeIsOnlyValidAsATargetNode()
        {
            var baseNodeType = typeof(Foo);
            var relationship = new TestRelationship(new NodeReference<Baz>(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.Equal(RelationshipDirection.Outgoing, calculatedDirection);
        }

        [Fact]
        public void DetermineRelationshipDirectionShouldReturnOutgoingWhenBaseNodeIsOnlyValidAsASourceNodeEvenIfOtherNodeIsAlsoValidAsASourceNode()
        {
            var baseNodeType = typeof(Foo);
            var relationship = new TestRelationship(new NodeReference<Bar>(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.Equal(RelationshipDirection.Outgoing, calculatedDirection);
        }

        [Fact]
        public void DetermineRelationshipDirectionShouldReturnIncomingWhenBaseNodeIsOnlyValidAsATargetNodeAndOtherNodeIsOnlyValidAsASourceNode()
        {
            var baseNodeType = typeof(Baz);
            var relationship = new TestRelationship(new NodeReference<Foo>(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.Equal(RelationshipDirection.Incoming, calculatedDirection);
        }

        [Fact]
        public void DetermineRelationshipDirectionShouldReturnIncomingWhenBaseNodeIsOnlyValidAsATargetNodeEvenIfOtherNodeIsAlsoValidAsATargetNode()
        {
            var baseNodeType = typeof(Baz);
            var relationship = new TestRelationship(new NodeReference<Bar>(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.Equal(RelationshipDirection.Incoming, calculatedDirection);
        }

        [Fact]
        public void DetermineRelationshipDirectionShouldThrowExceptionWhenBothNodesAreValidAsSourceAndTargetNodes()
        {
            var baseNodeType = typeof(Bar);
            var relationship = new TestRelationship(new NodeReference<Bar>(123));
            Assert.Throws<AmbiguousRelationshipDirectionException>(() => Relationship.DetermineRelationshipDirection(baseNodeType, relationship));
        }

        [Fact]
        public void DetermineRelationshipDirectionShouldReturnIncomingWhenBaseNodeOnlyValidAsTargetAndSourceNodeNotDefinedAsEither()
        {
            var baseNodeType = typeof(Baz);
            var relationship = new TestRelationship(new NodeReference<Qak>(123));
            Assert.Throws<AmbiguousRelationshipDirectionException>(() => Assert.Equal(RelationshipDirection.Incoming, Relationship.DetermineRelationshipDirection(baseNodeType, relationship)));
        }

        [Fact]
        public void DetermineRelationshipDirectionShouldThrowExceptionWhenNeitherNodeIsValidAtEitherEnd()
        {
            var baseNodeType = typeof(Zip);
            var relationship = new TestRelationship(new NodeReference<Qak>(123));
            Assert.Throws<AmbiguousRelationshipDirectionException>(() => Relationship.DetermineRelationshipDirection(baseNodeType, relationship));
        }

        [Fact]
        public void DetermineRelationshipDirectionShouldReturnOutgoingWhenBaseNodeIsValidAsASourceNodeAndOtherNodeIsAnUntypedReference()
        {
            var baseNodeType = typeof(Bar);
            var relationship = new TestRelationship(new NodeReference(123));
            var calculatedDirection = Relationship.DetermineRelationshipDirection(baseNodeType, relationship);

            Assert.Equal(RelationshipDirection.Outgoing, calculatedDirection);
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