using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using System.Linq;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ExecuteGetAllRelationshipsGremlinTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.ExecuteGetAllRelationshipsGremlin("", null);
        }

        [Test]
        public void ShouldReturnListOfRelationshipInstances()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";

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
                    }.AddBody(new GremlinApiQuery(gremlinQueryExpected, null)),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =@"[ {
                          'start' : 'http://127.0.0.1:5118/db/data/node/123',
                          'data' : {
                          },
                          'self' : 'http://127.0.0.1:5118/db/data/relationship/456',
                          'property' : 'http://127.0.0.1:5118/db/data/relationship/456/properties/{key}',
                          'properties' : 'http://127.0.0.1:5118/db/data/relationship/456/properties',
                          'type' : 'KNOWS',
                          'extensions' : {
                          },
                          'end' : 'http://127.0.0.1:5118/db/data/node/789'
                        } ]"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var relationships = graphClient
                .ExecuteGetAllRelationshipsGremlin(gremlinQueryExpected, null)
                .ToList();

            //Assert
            Assert.AreEqual(1, relationships.Count());
            Assert.AreEqual(456, relationships.ElementAt(0).Reference.Id);
            Assert.AreEqual(123, relationships.ElementAt(0).StartNodeReference.Id);
            Assert.AreEqual(789, relationships.ElementAt(0).EndNodeReference.Id);
        }

        [Test]
        public void ShouldReturnEmptyEnumerableForNullResult()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";

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
                    }.AddBody(new GremlinApiQuery(gremlinQueryExpected, null)),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =@"[]"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodes = graphClient
                .ExecuteGetAllRelationshipsGremlin(gremlinQueryExpected, null)
                .ToList();

            //Assert
            Assert.AreEqual(0, nodes.Count());
        }
    }
}