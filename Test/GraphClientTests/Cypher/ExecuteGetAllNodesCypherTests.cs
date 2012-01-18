using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    public class Foo
    {
        public string Bar { get; set; }
        public string Baz { get; set; }
    }

    [TestFixture]
    public class ExecuteGetAllNodesCypherTests
    {
        [Test]
        public void ShouldReturnIEnumerableOfObjects()
        {
            //Arrange
            const string cypherExpectedQuery = "start myNode=node({foo}) return myNode";
            var parameters = new Dictionary<string, object>
                {
                    {"foo", 123}
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
                          'cypher' : 'http://foo/db/data/cypher',
                          'batch' : 'http://foo/db/data/batch',
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
                        Resource = "/cypher",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new CypherApiQuery("start myNode=node({foo}) return myNode", parameters)),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =@"{
                                      'data' : [ [ {
                                        'outgoing_relationships' : 'http://foo/db/data/node/756/relationships/out',
                                        'data' : {
                                          'Bar' : 'bar',
                                          'Baz' : 'baz'
                                        },
                                        'traverse' : 'http://foo/db/data/node/756/traverse/{returnType}',
                                        'all_typed_relationships' : 'http://foo/db/data/node/756/relationships/all/{-list|&|types}',
                                        'self' : 'http://foo/db/data/node/756',
                                        'property' : 'http://foo/db/data/node/756/properties/{key}',
                                        'properties' : 'http://foo/db/data/node/756/properties',
                                        'outgoing_typed_relationships' : 'http://foo/db/data/node/756/relationships/out/{-list|&|types}',
                                        'incoming_relationships' : 'http://foo/db/data/node/756/relationships/in',
                                        'extensions' : {
                                        },
                                        'create_relationship' : 'http://foo/db/data/node/756/relationships',
                                        'paged_traverse' : 'http://foo/db/data/node/756/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                        'all_relationships' : 'http://foo/db/data/node/756/relationships/all',
                                        'incoming_typed_relationships' : 'http://foo/db/data/node/756/relationships/in/{-list|&|types}'
                                      },{
                                        'outgoing_relationships' : 'http://foo/db/data/node/757/relationships/out',
                                        'data' : {
                                          'Bar' : '123',
                                          'Baz' : '456'
                                        },
                                        'traverse' : 'http://foo/db/data/node/757/traverse/{returnType}',
                                        'all_typed_relationships' : 'http://foo/db/data/node/757/relationships/all/{-list|&|types}',
                                        'self' : 'http://foo/db/data/node/757',
                                        'property' : 'http://foo/db/data/node/757/properties/{key}',
                                        'properties' : 'http://foo/db/data/node/757/properties',
                                        'outgoing_typed_relationships' : 'http://foo/db/data/node/757/relationships/out/{-list|&|types}',
                                        'incoming_relationships' : 'http://foo/db/data/node/757/relationships/in',
                                        'extensions' : {
                                        },
                                        'create_relationship' : 'http://foo/db/data/node/757/relationships',
                                        'paged_traverse' : 'http://foo/db/data/node/757/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                        'all_relationships' : 'http://foo/db/data/node/757/relationships/all',
                                        'incoming_typed_relationships' : 'http://foo/db/data/node/757/relationships/in/{-list|&|types}'
                                      } ] ],
                                      'columns' : [ 'myNode' ]
                                    }"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var nodes = graphClient
                .ExecuteGetAllNodesCypher<Foo>(cypherExpectedQuery, parameters)
                .ToList();

            //Assert
            Assert.AreEqual(2, nodes.Count());
            Assert.AreEqual(756, nodes.ElementAt(0).Reference.Id);
            Assert.AreEqual("bar", nodes.ElementAt(0).Data.Bar);
            Assert.AreEqual("baz", nodes.ElementAt(0).Data.Baz);
            Assert.AreEqual(757, nodes.ElementAt(1).Reference.Id);
            Assert.AreEqual("123", nodes.ElementAt(1).Data.Bar);
            Assert.AreEqual("456", nodes.ElementAt(1).Data.Baz);
        }
    }
}
