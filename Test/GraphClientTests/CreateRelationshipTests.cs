using System;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class CreateRelationshipTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionForNullNodeReference()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.CreateRelationship((NodeReference<TestNode>)null, new TestRelationship(10));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.CreateRelationship(new NodeReference<TestNode>(5), new TestRelationship(10));
        }

        public class TestNode
        {
        }

        public class TestNode2
        {
        }

        public class TestRelationship : Relationship,
            IRelationshipAllowingSourceNode<TestNode>,
            IRelationshipAllowingTargetNode<TestNode2>
        {
            public TestRelationship(NodeReference targetNode)
                : base(targetNode)
            {
            }

            public override string RelationshipTypeKey
            {
                get { return "TEST_RELATIONSHIP"; }
            }
        }
    }
}
