using System;
using System.Net;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class CreateRelationshipTests
    {
        [Test]
        public void ShouldReturnRelationshipReference()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson("/node/81/relationships",
                        @"{
                            'to': 'http://foo/db/data/node/81',
                            'type': 'TEST_RELATIONSHIP'
                        }"),
                    MockResponse.Json(HttpStatusCode.Created,
                        @"{
                            'extensions' : {
                            },
                            'start' : 'http://foo/db/data/node/81',
                            'property' : 'http://foo/db/data/relationship/38/properties/{key}',
                            'self' : 'http://foo/db/data/relationship/38',
                            'properties' : 'http://foo/db/data/relationship/38/properties',
                            'type' : 'TEST_RELATIONSHIP',
                            'end' : 'http://foo/db/data/node/80',
                            'data' : {
                            }
                        }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                var testRelationship = new TestRelationship(81);
                var relationshipReference = graphClient.CreateRelationship(new NodeReference<TestNode>(81), testRelationship);

                Assert.IsInstanceOf<RelationshipReference>(relationshipReference);
                Assert.IsNotInstanceOf<RelationshipReference<object>>(relationshipReference);
                Assert.AreEqual(38, relationshipReference.Id);
            }
        }

        [Test]
        public void ShouldReturnAttachedRelationshipReference()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson("/node/81/relationships",
                        @"{
                            'to': 'http://foo/db/data/node/81',
                            'type': 'TEST_RELATIONSHIP'
                        }"),
                    MockResponse.Json(HttpStatusCode.Created,
                        @"{
                            'extensions' : {
                            },
                            'start' : 'http://foo/db/data/node/81',
                            'property' : 'http://foo/db/data/relationship/38/properties/{key}',
                            'self' : 'http://foo/db/data/relationship/38',
                            'properties' : 'http://foo/db/data/relationship/38/properties',
                            'type' : 'TEST_RELATIONSHIP',
                            'end' : 'http://foo/db/data/node/80',
                            'data' : {
                            }
                        }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                var testRelationship = new TestRelationship(81);
                var relationshipReference = graphClient.CreateRelationship(new NodeReference<TestNode>(81), testRelationship);

                Assert.AreEqual(graphClient, ((IAttachedReference)relationshipReference).Client);
            }
        }

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

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ShouldThrowNotSupportedExceptionForIncomingRelationship()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectGraphClient();
                client.CreateRelationship(new NodeReference<TestNode>(5), new TestRelationship(10) { Direction = RelationshipDirection.Incoming });
            }
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
