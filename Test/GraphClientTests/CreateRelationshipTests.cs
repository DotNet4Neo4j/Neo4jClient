using System;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class CreateRelationshipTests
    {
        public class TestRelationship : IRelationshipType
        {
            public string TypeKey
            {
                get { return "Test"; }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionForNullSourceNode()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.CreateRelationship<TestRelationship>(null, 456);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionForNullTargetNode()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.CreateRelationship<TestRelationship>(123, null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Create(new object());
        }
    }
}