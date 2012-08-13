using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class CreateRelationshipTests
    {
        readonly string rootResponse = @"{
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/0',
                'neo4j_version' : '1.5.M02',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {
                }
            }"
            .Replace('\'', '"');

        [Test]
        public void ShouldReturnRelationshipReference()
        {
            var testRelationship = new TestRelationship(81);

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    MockResponse.NeoRoot()
                },
                {
                    new RestRequest {
                        Resource = "/node/81/relationships",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new RelationshipTemplate
                    {
                        To = "http://foo/db/data/node/81",
                        Type = testRelationship.RelationshipTypeKey
                    }),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        TestContent = @"{
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
                                    }".Replace('\'', '\"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            var relationshipReference = graphClient.CreateRelationship(new NodeReference<TestNode>(81), testRelationship);

            Assert.IsInstanceOf<RelationshipReference>(relationshipReference);
            Assert.IsNotInstanceOf<RelationshipReference<object>>(relationshipReference);
            Assert.AreEqual(38, relationshipReference.Id);
        }

        [Test]
        public void ShouldReturnAttachedRelationshipReference()
        {
            var testRelationship = new TestRelationship(81);

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    MockResponse.NeoRoot()
                },
                {
                    new RestRequest {
                        Resource = "/node/81/relationships",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new RelationshipTemplate
                    {
                        To = "http://foo/db/data/node/81",
                        Type = testRelationship.RelationshipTypeKey
                    }),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        TestContent = @"{
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
                                    }".Replace('\'', '\"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            var relationshipReference = graphClient.CreateRelationship(new NodeReference<TestNode>(81), testRelationship);

            Assert.AreEqual(graphClient, ((IAttachedReference)relationshipReference).Client);
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
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = rootResponse
                    }
                }
            });

            var client = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            client.Connect();
            client.CreateRelationship(new NodeReference<TestNode>(5), new TestRelationship(10) { Direction = RelationshipDirection.Incoming });
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
