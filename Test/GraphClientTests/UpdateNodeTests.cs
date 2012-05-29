using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Neo4jClient.Serializer;
using Newtonsoft.Json;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class UpdateNodeTests
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
        public void ShouldUpdateNode()
        {
            var nodeToUpdate = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };

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
                },
                 {
                    new RestRequest { Resource = "/node/456", Method = Method.GET },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo',
                                    'Bar': 'bar',
                                    'Baz': 'baz'
                          },
                          'create_relationship': 'http://foo/db/data/node/456/relationships',
                          'all_relationships': 'http://foo/db/data/node/456/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/456/properties',
                          'property': 'http://foo/db/data/node/456/property/{key}',
                          'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/node/456/properties",
                        Method = Method.PUT,
                        RequestFormat = DataFormat.Json
                    }.AddBody(nodeToUpdate),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            var pocoReference = new NodeReference<TestNode>(456);
            graphClient.Update(
                pocoReference, nodeFromDb =>
                    {
                        nodeFromDb.Foo = "fooUpdated";
                        nodeFromDb.Baz = "bazUpdated";
                        nodeToUpdate = nodeFromDb;
                    }
                );

            Assert.AreEqual("fooUpdated", nodeToUpdate.Foo);
            Assert.AreEqual("bazUpdated", nodeToUpdate.Baz);
            Assert.AreEqual("bar", nodeToUpdate.Bar);
        }

        [Test]
        public void ShouldUpdateNodeWithIndexEntries()
        {
            var nodeToUpdate = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };

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
                },
                 {
                    new RestRequest { Resource = "/node/456", Method = Method.GET },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo',
                                    'Bar': 'bar',
                                    'Baz': 'baz'
                          },
                          'create_relationship': 'http://foo/db/data/node/456/relationships',
                          'all_relationships': 'http://foo/db/data/node/456/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/456/properties',
                          'property': 'http://foo/db/data/node/456/property/{key}',
                          'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/node/456/properties",
                        Method = Method.PUT,
                        RequestFormat = DataFormat.Json
                    }.AddBody(nodeToUpdate),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent
                    }
                }, 
                {
                    new RestRequest("/index/node/foo", Method.POST) {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                    }
                    .AddBody(new { key="foo", value="bar", uri="http://foo/db/data/node/456"}),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                        TestContent = "Location: http://foo/db/data/index/node/foo/foo/bar/456"
                    }
                },
                {
                   new RestRequest("/index/node/foo/456", Method.DELETE) {
                        RequestFormat = DataFormat.Json,
                        JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
                        },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent,
                        ContentType = "application/json",
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            var pocoReference = new NodeReference<TestNode>(456);
            graphClient.Update(
                pocoReference, nodeFromDb =>
                {
                    nodeFromDb.Foo = "fooUpdated";
                    nodeFromDb.Baz = "bazUpdated";
                    nodeToUpdate = nodeFromDb;
                }, nodeFromDb => new List<IndexEntry>
                    {
                        new IndexEntry
                            {
                                Name = "foo", 
                                KeyValues = new Dictionary<string, object> {{"foo", "bar"}},
                            }
                    });

            Assert.AreEqual("fooUpdated", nodeToUpdate.Foo);
            Assert.AreEqual("bazUpdated", nodeToUpdate.Baz);
            Assert.AreEqual("bar", nodeToUpdate.Bar);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ShouldThrowNotSupportedExceptionForPre15M02DatabaseWithIndexEntries()
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
                },
                {
                    new RestRequest { Resource = "/node/456", Method = Method.GET },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{ 'self': 'http://foo/db/data/node/456',
                            'data': { 'Foo': 'foo',
                                    'Bar': 'bar',
                                    'Baz': 'baz'
                            },
                            'create_relationship': 'http://foo/db/data/node/456/relationships',
                            'all_relationships': 'http://foo/db/data/node/456/relationships/all',
                            'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
                            'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
                            'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
                            'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
                            'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
                            'properties': 'http://foo/db/data/node/456/properties',
                            'property': 'http://foo/db/data/node/456/property/{key}',
                            'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
                        }".Replace('\'', '"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            var pocoReference = new NodeReference<TestNode>(456);
            graphClient.Update(
                pocoReference,
                nodeFromDb =>
                {
                    nodeFromDb.Foo = "fooUpdated";
                    nodeFromDb.Baz = "bazUpdated";
                },
                nodeFromDb => new List<IndexEntry>
                {
                    new IndexEntry
                    {
                        Name = "foo",
                        KeyValues = new Dictionary<string, object> {{"foo", "bar"}},
                    }
                });
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }
    }
}