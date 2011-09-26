using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using Neo4jClient.Gremlin;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class CreateNodeTests
    {
        readonly string rootResponse = @"{
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/0',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions'' : {
                }
            }"
            .Replace('\'', '"');

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
        [ExpectedException(typeof(ValidationException))]
        public void ShouldThrowValidationExceptionForInvalidNodes()
        {
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), null);

            var testNode = new TestNode {Foo = "text is too long", Bar = null, Baz = "123"};
            graphClient.Create(testNode);
        }

        [Test]
        public void ShouldReturnReferenceToCreatedNode()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(Method.POST, "/node", testNode);

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse { StatusCode = HttpStatusCode.OK, ContentType = "application/json", Content = rootResponse }
                },
                {
                    new RestRequest {
                        Resource = "/batch",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(batch),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/760/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/760/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/760/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/760',
                          'property' : 'http://foo/db/data/node/760/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/760/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/760/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/760/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/760/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/760/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/760/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/760/relationships/in/{-list|&|types}'
                        },'from':'/node'}]".Replace('\'', '\"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            var node = graphClient.Create(testNode);

            Assert.AreEqual(760, node.Id);
        }

        [Test]
        [Ignore]
        public void ShouldReturnAttachedNodeReference()
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
                          'batch' : 'http://foo/db/data/batch',
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

            Assert.IsNotNull(((IGremlinQuery)node).Client);
        }

        [Test]
        [Ignore]
        public void ShouldCreateOutgoingRelationship()
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
                          'batch' : 'http://foo/db/data/batch',
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
                    .AddBody(new RelationshipTemplate
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

        [Test]
        [Ignore]
        public void ShouldCreateIncomingRelationship()
        {
            var testNode2 = new TestNode2 { Foo = "foo", Bar = "bar" };
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
                          'batch' : 'http://foo/db/data/batch',
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
                    }.AddBody(testNode2),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        Headers = {
                            new HttpHeader { Name = "Location", Value ="http://foo/db/data/node/456" }
                        }
                    }
                },
                {
                    new RestRequest {
                        Resource = "/node/789/relationships",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }
                    .AddBody(new RelationshipTemplate
                    {
                        To = "http://foo/db/data/node/456",
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
                testNode2,
                new TestRelationship(789, testPayload));

            Assert.Inconclusive("Not actually asserting that the relationship was created");
        }

        public class TestNode
        {
            [StringLength(4)]
            public string Foo { get; set; }

            [Required]
            public string Bar { get; set; }

            [RegularExpression(@"\w*")]
            public string Baz { get; set; }
        }

        public class TestNode2
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        public class TestPayload
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        public class TestRelationship : Relationship<TestPayload>,
            IRelationshipAllowingSourceNode<TestNode>,
            IRelationshipAllowingTargetNode<TestNode2>
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