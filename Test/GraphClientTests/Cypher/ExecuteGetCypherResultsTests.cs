using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class ExecuteGetCypherResultsTests
    {
        public class Foo
        {
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        [Test]
        public void StartShouldReturnCypherTableResults()
        {

            //Arrange
            var cypherExpectedQuery = @"{'query': 'start x  = node({foo}) match x -[r]-> n return  x, type(r), n.Name?, n.UniqueId? limit 3','params': {'foo':0}},".Replace('\'', '"');
            var parameters = new Dictionary<string, object>
                {
                    {"foo", 123}
                };

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
                {
                    {
                        new RestRequest
                            {
                                Resource = "/",
                                Method = Method.GET
                            },
                        new HttpResponse
                            {
                                StatusCode = HttpStatusCode.OK,
                                ContentType =
                        "application/json",
                                Content =
                        @"{
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
                        new RestRequest
                            {
                                Resource = "/cypher",
                                Method = Method.POST,
                                RequestFormat = DataFormat.Json
                            }.AddBody(
                                new CypherApiQuery(
                                    "start myNode=node({foo}) return myNode",
                                    parameters)),
                        new HttpResponse
                            {
                                StatusCode = HttpStatusCode.OK,
                                ContentType =
                        "application/json",
                                Content =
                        @"{
                            'data' : [ [ {
                            'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                            'data' : {
                                'Bar' : 'bar',
                                'Baz' : 'baz'
                            },
                            'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                            'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                            'property' : 'http://foo/db/data/node/0/properties/{key}',
                            'self' : 'http://foo/db/data/node/0',
                            'properties' : 'http://foo/db/data/node/0/properties',
                            'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                            'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                            'extensions' : {
                            },
                            'create_relationship' : 'http://foo/db/data/node/0/relationships',
                            'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                            'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                            'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                            }, 'HOSTS', 'foo', 44321 ], [ {
                            'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                            'data' : {
                                'Bar' : 'bar',
                                'Baz' : 'baz'
                            },
                            'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                            'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                            'property' : 'http://foo/db/data/node/0/properties/{key}',
                            'self' : 'http://foo/db/data/node/0',
                            'properties' : 'http://foo/db/data/node/0/properties',
                            'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                            'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                            'extensions' : {
                            },
                            'create_relationship' : 'http://foo/db/data/node/0/relationships',
                            'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                            'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                            'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                            }, 'HOSTS', 'bar', 44311 ], [ {
                            'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                            'data' : {
                                'Bar' : 'bar',
                                'Baz' : 'baz'
                            },
                            'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                            'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                            'property' : 'http://foo/db/data/node/0/properties/{key}',
                            'self' : 'http://foo/db/data/node/0',
                            'properties' : 'http://foo/db/data/node/0/properties',
                            'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                            'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                            'extensions' : {
                            },
                            'create_relationship' : 'http://foo/db/data/node/0/relationships',
                            'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                            'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                            'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                            }, 'HOSTS', 'baz', 42586 ] ],
                            'columns' : [ 'x', 'TYPE(r)', 'n.Name', 'n.UniqueId' ]
                        }".Replace('\'', '"')
                            }
                        }
                });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var cypherResult = graphClient
                .ExecuteGetCypherResults<CypherApiTableResponse<NodeApiResponse<Foo>, RelationshipApiResponse<Foo>, string>>(
                    new CypherQuery(graphClient, cypherExpectedQuery, parameters,null)
                );

            //Assert
            Assert.AreEqual(4, cypherResult.Columns.Count());
            //ToDo Fill in additional asserts

        }
    }
}