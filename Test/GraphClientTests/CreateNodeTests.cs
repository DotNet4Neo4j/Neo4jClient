using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class CreateNodeTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowArgumentNullExceptionForNullNode()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Create<object>(null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Create(new object());
        }

        [Test]
        public void ShouldReturnReferenceToCreatedNode()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions'' : {
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/node",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(testNode),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        Headers = {
                            new HttpHeader { Name = "Location", Value ="http://foo/db/data/node/456" }
                        }
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            var node = graphClient.Create(testNode);

            Assert.AreEqual(456, node.Id);
        }

        [Test]
        public void ShouldCreateRelationship()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var testPayload = new TestPayload { Foo = "123", Bar = "456", Baz = "789" };

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions'' : {
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/node",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(testNode),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        Headers = {
                            new HttpHeader { Name = "Location", Value ="http://foo/db/data/node/456" }
                        }
                    }
                },
                {
                    new RestRequest {
                        Resource = "/node/456/relationships",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }
                    .AddBody(new RelationshipPacket
                    {
                        To = "http://foo/db/data/node/789",
                        Data = testPayload,
                        Type = "TEST_RELATIONSHIP"
                    }),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        Headers = {
                            new HttpHeader { Name = "Location", Value ="http://foo/db/data/relationship/123" }
                        }
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            graphClient.Create(
                testNode,
                new TestRelationship(789, testPayload));

            Assert.Inconclusive("Not actually asserting that the relationship was created");
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        public class TestPayload
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        public class TestRelationship : Relationship<TestPayload>,
            IRelationshipAllowingSourceNode<TestNode>
        {
            public TestRelationship(NodeReference targetNode, TestPayload data)
                : base(targetNode, data)
            {
            }

            public override string RelationshipTypeKey
            {
                get { return "TEST_RELATIONSHIP"; }
            }
        }
    }
}