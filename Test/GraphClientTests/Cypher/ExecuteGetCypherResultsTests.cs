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
            const string cypherExpectedQuery = "start myNode=node({foo}) return myNode";
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
                        @"{'data' : [
                                        [
                                        {
                                        'column1': {
                                                    'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                                                    'data' : {
                                                        'Bar' : '1',
                                                        'Baz' : '11'
                                                    },
                                                    'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                                                    'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                                                    'self' : 'http://foo/db/data/node/0',
                                                    'property' : 'http://foo/db/data/node/0/properties/{key}',
                                                    'properties' : 'http://foo/db/data/node/0/properties',
                                                    'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                                                    'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                                                    'extensions' : {
                                                    },
                                                    'create_relationship' : 'http://foo/db/data/node/0/relationships',
                                                    'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                                    'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                                                    'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                                                  },
                                        'column2': {
                                                    'start' : 'http://foo/db/data/node/2',
                                                    'data' : {
                                                        'Bar' : '2',
                                                        'Baz' : '22'
                                                    },
                                                    'property' : 'http://foo/db/data/relationship/6/properties/{key}',
                                                    'self' : 'http://foo/db/data/relationship/6',
                                                    'properties' : 'http://foo/db/data/relationship/6/properties',
                                                    'type' : 'HAS_LANGUAGE',
                                                    'extensions' : {
                                                    },
                                                    'end' : 'http://foo/db/data/node/7'
                                                  } ,
                                         'column3': 'mystring'
                                        },
                                        {
                                        'column1': {
                                                    'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                                                    'data' : {
                                                        'Bar' : '1',
                                                        'Baz' : '11'
                                                    },
                                                    'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                                                    'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                                                    'self' : 'http://foo/db/data/node/0',
                                                    'property' : 'http://foo/db/data/node/0/properties/{key}',
                                                    'properties' : 'http://foo/db/data/node/0/properties',
                                                    'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                                                    'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                                                    'extensions' : {
                                                    },
                                                    'create_relationship' : 'http://foo/db/data/node/0/relationships',
                                                    'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                                    'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                                                    'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                                                  },
                                        'column2': {
                                                    'start' : 'http://foo/db/data/node/2',
                                                    'data' : {
                                                        'Bar' : '2',
                                                        'Baz' : '22'
                                                    },
                                                    'property' : 'http://foo/db/data/relationship/6/properties/{key}',
                                                    'self' : 'http://foo/db/data/relationship/6',
                                                    'properties' : 'http://foo/db/data/relationship/6/properties',
                                                    'type' : 'HAS_LANGUAGE',
                                                    'extensions' : {
                                                    },
                                                    'end' : 'http://foo/db/data/node/7'
                                                  },
                                        'column3': 'mystring'
                                        }
                                        ]
                                    ],
                          'columns' : [ 'Column1FriendlyName', 'Column2FriendlyName', 'Column3FirendlyName' ]}".Replace('\'', '"')
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
            Assert.AreEqual(3, cypherResult.Columns.Count());
            Assert.AreEqual(1, cypherResult.Data.Count());

            Assert.AreEqual(0, cypherResult.Data.First().First().Column1.ToNode(null).Reference.Id);
            Assert.AreEqual("1", cypherResult.Data.First().First().Column1.Data.Bar);
            Assert.AreEqual("11", cypherResult.Data.First().First().Column1.Data.Baz);

            Assert.AreEqual(6, cypherResult.Data.First().First().Column2.ToRelationshipInstance(null).Reference.Id);
            Assert.AreEqual("2", cypherResult.Data.First().First().Column2.Data.Bar);
            Assert.AreEqual("22", cypherResult.Data.First().First().Column2.Data.Baz);

            Assert.AreEqual("mystring",cypherResult.Data.First().First().Column3);

        }
    }
}