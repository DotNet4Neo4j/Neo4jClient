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

        public class Foo { }
        public class Bar { }
        public class Baz { }

        public class TestRelationship : Relationship,
            IRelationshipAllowingSourceNode<Foo>,
            IRelationshipAllowingSourceNode<Bar>,
            IRelationshipAllowingTargetNode<Bar>,
            IRelationshipAllowingTargetNode<Baz>
        {
            public TestRelationship(NodeReference targetNode) : base(targetNode)
            {
            }

            public TestRelationship(NodeReference targetNode, object data) : base(targetNode, data)
            {
            }

            public override string RelationshipTypeKey
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}