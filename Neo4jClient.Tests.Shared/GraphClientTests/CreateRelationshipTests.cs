using System;
using System.Net;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class CreateRelationshipTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
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

                Assert.IsAssignableFrom<RelationshipReference>(relationshipReference);
                Assert.IsNotType<RelationshipReference<object>>(relationshipReference);
                Assert.Equal(38, relationshipReference.Id);
            }
        }

        [Fact]
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

                Assert.Equal(graphClient, ((IAttachedReference)relationshipReference).Client);
            }
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionForNullNodeReference()
        {
            var client = new GraphClient(new Uri("http://foo"));
            Assert.Throws<ArgumentNullException>(() => client.CreateRelationship((NodeReference<TestNode>)null, new TestRelationship(10)));
        }

        [Fact]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            Assert.Throws<InvalidOperationException>(() => client.CreateRelationship(new NodeReference<TestNode>(5), new TestRelationship(10)));
        }

        [Fact]
        public void ShouldThrowNotSupportedExceptionForIncomingRelationship()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectGraphClient();
                Assert.Throws<NotSupportedException>(() => client.CreateRelationship(new NodeReference<TestNode>(5), new TestRelationship(10) { Direction = RelationshipDirection.Incoming }));
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
