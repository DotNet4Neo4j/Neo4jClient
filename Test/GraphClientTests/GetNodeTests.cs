using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Neo4jClient.Gremlin;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class GetNodeTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Get<object>(123);
        }

        [Test]
        public void ShouldReturnNodeData()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
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
            var node = graphClient.Get<TestNode>(456);

            Assert.AreEqual(456, node.Reference.Id);
            Assert.AreEqual("foo", node.Data.Foo);
            Assert.AreEqual("bar", node.Data.Bar);
            Assert.AreEqual("baz", node.Data.Baz);
        }

        [Test]
        public void ShouldReturnNodeDataAndDeserializeToEnumType()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest { Resource = "/node/456", Method = Method.GET },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo',
                                    'Status': 'Value1'
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
            var node = graphClient.Get<TestNodeWithEnum>(456);

            Assert.AreEqual(TestEnum.Value1, node.Data.Status);
        }

        [Test]
        public void ShouldReturnNodeWithReferenceBackToClient()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
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
            var node = graphClient.Get<TestNode>(456);

            Assert.AreEqual(graphClient, ((IGremlinQuery) node.Reference).Client);
        }

        [Test]
        public void ShouldReturnNullWhenNodeDoesntExist()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest { Resource = "/node/456", Method = Method.GET },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NotFound,
                        StatusDescription = "Node not found"
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
            var node = graphClient.Get<TestNode>(456);

            Assert.IsNull(node);
        }

        [Test]
        public void ShouldReturnNodeDataAndDeserialzedJsonDatesForDateTimeOffsetNullableType()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest { Resource = "/node/456", Method = Method.GET},
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo',
                                    'Bar': 'bar',
                                    'Baz': 'baz',
                                    'DateOffSet': '/Date(1309421746929+0000)/'
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
            var node = graphClient.Get<TestNode>(456);

            Assert.IsNotNull(node.Data.DateOffSet);
            Assert.AreEqual("2011-06-30 08:15:46Z", node.Data.DateOffSet.Value.ToString("u"));
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
            public DateTimeOffset? DateOffSet { get; set; }
        }

        public class TestNodeWithEnum
        {
            public string Foo { get; set; }
            public TestEnum Status { get; set; }
        }

        public enum TestEnum
        {
            Value1,
            Value2
        }
    }
}
