using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using NUnit.Framework;
using System.Linq;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ExecuteGetAllNodesGremlinTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.ExecuteScalarGremlin("", null);
        }

        [Test]
        public void ShouldReturnIEnumerableOfObjects()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";
            var expectedNode1 = new NodePacket<String>
                                   {
                                       Data = "{\r\n  \"Name\": \"My Agency 1\",\r\n  \"Key\": \"MyAgency1\"\r\n}",
                                       Self = "http://foo/db/data/node/5" 
                                   };

            var expectedNode2 = new NodePacket<String>
            {
                Data = "{\r\n  \"Name\": \"My Agency 2\",\r\n  \"Key\": \"MyAgency2\"\r\n}",
                Self = "http://foo/db/data/node/6"
            };

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
                          'extensions' : {
                            'GremlinPlugin' : {
                              'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                            }
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/ext/GremlinPlugin/graphdb/execute_script",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddParameter("script", gremlinQueryExpected, ParameterType.GetOrPost),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =@"[ {
                            'outgoing_relationships' : 'http://foo/db/data/node/5/relationships/out',
                            'data' : {
                            'Name' : 'My Agency 1',
                            'Key' : 'MyAgency1'
                            },
                            'traverse' : 'http://foo/db/data/node/5/traverse/{returnType}',
                            'all_typed_relationships' : 'http://foo/db/data/node/5/relationships/all/{-list|&|types}',
                            'property' : 'http://foo/db/data/node/5/properties/{key}',
                            'self' : 'http://foo/db/data/node/5',
                            'properties' : 'http://foo/db/data/node/5/properties',
                            'outgoing_typed_relationships' : 'http://foo/db/data/node/5/relationships/out/{-list|&|types}',
                            'incoming_relationships' : 'http://foo/db/data/node/5/relationships/in',
                            'extensions' : {
                            },
                            'create_relationship' : 'http://foo/db/data/node/5/relationships',
                            'all_relationships' : 'http://foo/db/data/node/5/relationships/all',
                            'incoming_typed_relationships' : 'http://foo/db/data/node/5/relationships/in/{-list|&|types}'
                        }, {
                            'outgoing_relationships' : 'http://foo/db/data/node/6/relationships/out',
                            'data' : {
                            'Name' : 'My Agency 2',
                            'Key' : 'MyAgency2'
                            },
                            'traverse' : 'http://foo/db/data/node/6/traverse/{returnType}',
                            'all_typed_relationships' : 'http://foo/db/data/node/6/relationships/all/{-list|&|types}',
                            'property' : 'http://foo/db/data/node/6/properties/{key}',
                            'self' : 'http://foo/db/data/node/6',
                            'properties' : 'http://foo/db/data/node/6/properties',
                            'outgoing_typed_relationships' : 'http://foo/db/data/node/6/relationships/out/{-list|&|types}',
                            'incoming_relationships' : 'http://foo/db/data/node/6/relationships/in',
                            'extensions' : {
                            },
                            'create_relationship' : 'http://foo/db/data/node/6/relationships',
                            'all_relationships' : 'http://foo/db/data/node/6/relationships/all',
                            'incoming_typed_relationships' : 'http://foo/db/data/node/6/relationships/in/{-list|&|types}'
                        } ]"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodes = graphClient
                .ExecuteGetAllNodesGremlin<NodePacket<string>>(gremlinQueryExpected, new NameValueCollection())
                .ToList();

            //Assert
            Assert.IsNotEmpty(nodes.Where(x => x.Data == expectedNode1.Data && x.Self == expectedNode1.Self).ToList());
            Assert.IsNotEmpty(nodes.Where(x => x.Data == expectedNode2.Data && x.Self == expectedNode2.Self).ToList());
        }

        [Test]
        public void ShouldReturnEmptyEnumerableForNullResult()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";
            var expectedNode1 = new NodePacket<String>
            {
                Data = "{\r\n  \"Name\": \"My Agency 1\",\r\n  \"Key\": \"MyAgency1\"\r\n}",
                Self = "http://foo/db/data/node/5"
            };

            var expectedNode2 = new NodePacket<String>
            {
                Data = "{\r\n  \"Name\": \"My Agency 2\",\r\n  \"Key\": \"MyAgency2\"\r\n}",
                Self = "http://foo/db/data/node/6"
            };

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
                          'extensions' : {
                            'GremlinPlugin' : {
                              'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                            }
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/ext/GremlinPlugin/graphdb/execute_script",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddParameter("script", gremlinQueryExpected, ParameterType.GetOrPost),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =@""
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodes = graphClient
                .ExecuteGetAllNodesGremlin<NodePacket<string>>(gremlinQueryExpected, new NameValueCollection())
                .ToList();

            //Assert
            Assert.AreEqual(0, nodes.Count());
        }
    }
}