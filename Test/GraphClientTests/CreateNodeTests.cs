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
                'neo4j_version' : '1.5.M02',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {
                }
            }"
            .Replace('\'', '"');

        readonly string pre15M02RootResponse = @"{
                'batch' : 'http://foo/db/data/batch',
                'node' : 'http://foo/db/data/node',
                'node_index' : 'http://foo/db/data/index/node',
                'relationship_index' : 'http://foo/db/data/index/relationship',
                'reference_node' : 'http://foo/db/data/node/0',
                'extensions_info' : 'http://foo/db/data/ext',
                'extensions' : {
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
        [ExpectedException(typeof(NotSupportedException))]
        public void ShouldThrowNotSupportExceptionForPre15M02Database()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = pre15M02RootResponse
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            graphClient.Create(
                new object(),
                null,
                new[]
                {
                    new IndexEntry
                    {
                        Name = "my_index",
                        KeyValues = new[]
                        {
                            new KeyValuePair<string, object>("key", "value"),
                            new KeyValuePair<string, object>("key2", ""),
                            new KeyValuePair<string, object>("key3", "value3")
                        }
                    }
                });
        }

        [Test]
        public void ShouldNotThrowANotSupportedExceptionForPre15M02DatabaseWhenThereAreNoIndexEntries()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(Method.POST, "/node", testNode);

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = pre15M02RootResponse
                    }
                },
                {
                    new RestRequest {
                        Resource = "/batch",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(batch),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
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

            graphClient.Create(testNode, null, null);
        }

        [Test]
        public void ShouldReturnReferenceToCreatedNode()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(Method.POST, "/node", testNode);

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.OK, ContentType = "application/json", TestContent = rootResponse }
                },
                {
                    new RestRequest {
                        Resource = "/batch",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(batch),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
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
        public void ShouldReturnAttachedNodeReference()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(Method.POST, "/node", testNode);

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.OK, ContentType = "application/json", TestContent = rootResponse }
                },
                {
                    new RestRequest {
                        Resource = "/batch",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(batch),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
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

            Assert.IsNotNull(((IGremlinQuery)node).Client);
        }

        [Test]
        public void ShouldCreateOutgoingRelationship()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var testRelationshipPayload = new TestPayload { Foo = "123", Bar = "456", Baz = "789" };
            var batch = new List<BatchStep>();
            batch.Add(Method.POST, "/node", testNode);
            batch.Add(Method.POST, "{0}/relationships",
                new RelationshipTemplate { To = "/node/789", Data = testRelationshipPayload, Type = "TEST_RELATIONSHIP" });

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.OK, ContentType = "application/json", TestContent = rootResponse }
                },
                {
                    new RestRequest {
                        Resource = "/batch",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(batch),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
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
                        },'from':'/node'},{'id':1,'location':'http://foo/db/data/relationship/756','body':{
                          'start' : 'http://foo/db/data/node/760',
                          'data' : {
                            'Foo' : 123,
                            'Bar' : 456,
                            'Baz' : 789
                          },
                          'property' : 'http://foo/db/data/relationship/756/properties/{key}',
                          'self' : 'http://foo/db/data/relationship/756',
                          'properties' : 'http://foo/db/data/relationship/756/properties',
                          'type' : 'TEST_RELATIONSHIP',
                          'extensions' : {
                          },
                          'end' : 'http://foo/db/data/node/789'
                        },'from':'http://foo/db/data/node/761/relationships'}]".Replace('\'', '\"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            graphClient.Create(
                testNode,
                new TestRelationship(789, testRelationshipPayload));

            Assert.Inconclusive("Not actually asserting that the relationship was created");
        }

        [Test]
        public void ShouldCreateIndexEntries()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(Method.POST, "/node", testNode);
            batch.Add(Method.POST, "/index/node/my_index", new { key = "key", value = "value", uri = "{0}" });
            batch.Add(Method.POST, "/index/node/my_index", new { key = "key3", value = "value3", uri = "{0}" });

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.OK, ContentType = "application/json", TestContent = rootResponse }
                },
                {
                    new RestRequest {
                        Resource = "/batch",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(batch),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"[{'id':0,'location':'http://localhost:20001/db/data/node/763','body':{
                          'outgoing_relationships' : 'http://localhost:20001/db/data/node/763/relationships/out',
                          'data' : {
                            'Baz' : 'baz',
                            'Foo' : 'foo',
                            'Bar' : 'bar'
                          },
                          'traverse' : 'http://localhost:20001/db/data/node/763/traverse/{returnType}',
                          'all_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/all/{-list|&|types}',
                          'self' : 'http://localhost:20001/db/data/node/763',
                          'property' : 'http://localhost:20001/db/data/node/763/properties/{key}',
                          'outgoing_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/out/{-list|&|types}',
                          'properties' : 'http://localhost:20001/db/data/node/763/properties',
                          'incoming_relationships' : 'http://localhost:20001/db/data/node/763/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://localhost:20001/db/data/node/763/relationships',
                          'paged_traverse' : 'http://localhost:20001/db/data/node/763/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://localhost:20001/db/data/node/763/relationships/all',
                          'incoming_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/in/{-list|&|types}'
                        },'from':'/node'},{'id':1,'location':'http://localhost:20001/db/data/index/node/my_index/key/value/763','body':{
                          'indexed' : 'http://localhost:20001/db/data/index/node/my_index/key/value/763',
                          'outgoing_relationships' : 'http://localhost:20001/db/data/node/763/relationships/out',
                          'data' : {
                            'Baz' : 'baz',
                            'Foo' : 'foo',
                            'Bar' : 'bar'
                          },
                          'traverse' : 'http://localhost:20001/db/data/node/763/traverse/{returnType}',
                          'all_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/all/{-list|&|types}',
                          'self' : 'http://localhost:20001/db/data/node/763',
                          'property' : 'http://localhost:20001/db/data/node/763/properties/{key}',
                          'outgoing_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/out/{-list|&|types}',
                          'properties' : 'http://localhost:20001/db/data/node/763/properties',
                          'incoming_relationships' : 'http://localhost:20001/db/data/node/763/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://localhost:20001/db/data/node/763/relationships',
                          'paged_traverse' : 'http://localhost:20001/db/data/node/763/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://localhost:20001/db/data/node/763/relationships/all',
                          'incoming_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/in/{-list|&|types}'
                        },'from':'/index/node/my_index/key/value'}]".Replace('\'', '\"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            graphClient.Create(
                testNode,
                null,
                new[]
                {
                    new IndexEntry
                    {
                        Name = "my_index",
                        KeyValues = new[]
                        {
                            new KeyValuePair<string, object>("key", "value"),
                            new KeyValuePair<string, object>("key2", ""),
                            new KeyValuePair<string, object>("key3", "value3")
                        }
                    }
                });
        }

        [Test]
        public void ShouldCreateIncomingRelationship()
        {
            var testNode = new TestNode2 { Foo = "foo", Bar = "bar" };
            var testRelationshipPayload = new TestPayload { Foo = "123", Bar = "456", Baz = "789" };
            var batch = new List<BatchStep>();
            batch.Add(Method.POST, "/node", testNode);
            batch.Add(Method.POST, "/node/789/relationships",
                new RelationshipTemplate { To = "{0}", Data = testRelationshipPayload, Type = "TEST_RELATIONSHIP" });

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.OK, ContentType = "application/json", TestContent = rootResponse }
                },
                {
                    new RestRequest {
                        Resource = "/batch",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(batch),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
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
                        },'from':'/node'},{'id':1,'location':'http://foo/db/data/relationship/756','body':{
                          'start' : 'http://foo/db/data/node/760',
                          'data' : {
                            'Foo' : 123,
                            'Bar' : 456,
                            'Baz' : 789
                          },
                          'property' : 'http://foo/db/data/relationship/756/properties/{key}',
                          'self' : 'http://foo/db/data/relationship/756',
                          'properties' : 'http://foo/db/data/relationship/756/properties',
                          'type' : 'TEST_RELATIONSHIP',
                          'extensions' : {
                          },
                          'end' : 'http://foo/db/data/node/789'
                        },'from':'http://foo/db/data/node/761/relationships'}]".Replace('\'', '\"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            graphClient.Create(
                testNode,
                new TestRelationship(789, testRelationshipPayload));

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