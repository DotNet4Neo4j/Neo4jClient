﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Tests.Transactions;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests.Cypher
{
    
    public class ExecuteGetCypherResultsTests : IClassFixture<CultureInfoSetupFixture>
    {
        public class SimpleResultDto
        {
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        [Fact]
        public async Task EmptyCollectionShouldDeserializeCorrectly()
        {
            const string queryText = @"RETURN [] AS p";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Set, CypherResultFormat.Rest, "neo4j");
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
                {
                    {
                    MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"{'results': [{'columns' : [ 'p' ], 'data' : []}]}")
                    }
                })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                var results = (await graphClient
                    .ExecuteGetCypherResultsAsync<PathsResult>(cypherQuery))
                    .ToArray();

                Assert.Empty(results);
            }
        }

        [Fact(Skip = "Doesn't Reflect Current Response from Neo4j")]
        public async Task ShouldDeserializePathsResultAsSetBased()
        {
            // Arrange
            const string queryText = @"START d=node($p0), e=node($p1)
                                        MATCH p = allShortestPaths( d-[*..15]-e )
                                        RETURN p";

            var parameters = new Dictionary<string, object>
                {
                    {"p0", 215},
                    {"p1", 219}
                };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set, CypherResultFormat.Rest, "neo4j");
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
                {
                    {
                    MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                    MockResponse.Json(HttpStatusCode.OK,
                    @"{
                              'data' : [ [ {
                                'start' : 'http://foo/db/data/node/215',
                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/0', 'http://foo/db/data/node/219' ],
                                'length' : 2,
                                'relationships' : [ 'http://foo/db/data/relationship/247', 'http://foo/db/data/relationship/257' ],
                                'end' : 'http://foo/db/data/node/219'
                              } ], [ {
                                'start' : 'http://foo/db/data/node/215',
                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/1', 'http://foo/db/data/node/219' ],
                                'length' : 2,
                                'relationships' : [ 'http://foo/db/data/relationship/248', 'http://foo/db/data/relationship/258' ],
                                'end' : 'http://foo/db/data/node/219'
                              } ] ],
                              'columns' : [ 'p' ]
                            }")
                    }
                })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                //Act
                var results = (await graphClient
                    .ExecuteGetCypherResultsAsync<PathsResult>(cypherQuery))
                    .ToArray();

                //Assert
                Assert.IsAssignableFrom<IEnumerable<PathsResult>>(results);
                Assert.Equal(results.First().Length, 2);
                Assert.Equal(results.First().Start, "http://foo/db/data/node/215");
                Assert.Equal(results.First().End, "http://foo/db/data/node/219");
                Assert.Equal(results.Skip(1).First().Length, 2);
                Assert.Equal(results.Skip(1).First().Start, "http://foo/db/data/node/215");
                Assert.Equal(results.Skip(1).First().End, "http://foo/db/data/node/219");
            }
        }

        [Fact(Skip = "Doesn't Reflect Current Response from Neo4j")]
        public async Task ShouldDeserializeSimpleTableStructure()
        {
            // Arrange
            const string queryText = @"
                START x = node($p0)
                MATCH x-[r]->n
                RETURN type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var cypherQuery = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                },
                CypherResultMode.Projection,
                CypherResultFormat.Rest, "neo4j");

            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using(var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                        MockResponse.Json(HttpStatusCode.OK, @"{
                                'data' : [ [ 'HOSTS', 'foo', 44321 ], [ 'LIKES', 'bar', 44311 ], [ 'HOSTS', 'baz', 42586 ] ],
                                'columns' : [ 'RelationshipType', 'Name', 'UniqueId' ]
                            }")
                    }
                })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                //Act
                var results = await graphClient.ExecuteGetCypherResultsAsync<SimpleResultDto>(cypherQuery);

                //Assert
                Assert.IsAssignableFrom<IEnumerable<SimpleResultDto>>(results);

                var resultsArray = results.ToArray();
                Assert.Equal(3, resultsArray.Count());

                var firstResult = resultsArray[0];
                Assert.Equal("HOSTS", firstResult.RelationshipType);
                Assert.Equal("foo", firstResult.Name);
                Assert.Equal(44321, firstResult.UniqueId);

                var secondResult = resultsArray[1];
                Assert.Equal("LIKES", secondResult.RelationshipType);
                Assert.Equal("bar", secondResult.Name);
                Assert.Equal(44311, secondResult.UniqueId);

                var thirdResult = resultsArray[2];
                Assert.Equal("HOSTS", thirdResult.RelationshipType);
                Assert.Equal("baz", thirdResult.Name);
                Assert.Equal(42586, thirdResult.UniqueId);
            }
        }

        [Fact(Skip = "Doesn't Reflect Current Response from Neo4j")]
        public async Task ShouldDeserializeArrayOfNodesInPropertyAsResultOfCollectFunctionInCypherQuery()
        {
            // Arrange
            var cypherQuery = new CypherQuery(
                @"START root=node(0) MATCH root-[:HAS_COMPANIES]->()-[:HAS_COMPANY]->company, company--foo RETURN company, collect(foo) as Bar",
                new Dictionary<string, object>(),
                CypherResultMode.Projection,
                CypherResultFormat.Rest, "neo4j");

            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                        MockResponse.Json(HttpStatusCode.OK, @"{
  'columns' : [ 'ColumnA', 'ColumnBFromCollect' ],
  'data' : [ [ {
    'paged_traverse' : 'http://localhost:8000/db/data/node/358/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/358/relationships/out',
    'data' : {
      'Bar' : 'BHP',
      'Baz' : '1'
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/358/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/358/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/358',
    'property' : 'http://localhost:8000/db/data/node/358/properties/{key}',
    'all_relationships' : 'http://localhost:8000/db/data/node/358/relationships/all',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/358/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/358/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/358/relationships/in',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/358/relationships/in/{-list|&|types}',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/358/relationships'
  }, [ {
    'paged_traverse' : 'http://localhost:8000/db/data/node/362/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/362/relationships/out',
    'data' : {
      'OpportunityType' : 'Board',
      'Description' : 'Foo'
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/362/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/362/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/362',
    'property' : 'http://localhost:8000/db/data/node/362/properties/{key}',
    'all_relationships' : 'http://localhost:8000/db/data/node/362/relationships/all',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/362/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/362/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/362/relationships/in',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/362/relationships/in/{-list|&|types}',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/362/relationships'
  }, {
    'paged_traverse' : 'http://localhost:8000/db/data/node/359/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/359/relationships/out',
    'data' : {
      'OpportunityType' : 'Executive',
      'Description' : 'Bar'
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/359/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/359/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/359',
    'property' : 'http://localhost:8000/db/data/node/359/properties/{key}',
    'all_relationships' : 'http://localhost:8000/db/data/node/359/relationships/all',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/359/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/359/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/359/relationships/in',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/359/relationships/in/{-list|&|types}',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/359/relationships'
  } ] ] ]
}")
                    }
                })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                //Act
                var results = await graphClient.ExecuteGetCypherResultsAsync<CollectResult>(cypherQuery);

                //Assert
                Assert.IsAssignableFrom<IEnumerable<CollectResult>>(results);

                var resultsArray = results.ToArray();
                Assert.Equal(1, resultsArray.Count());

                var firstResult = resultsArray[0];
                Assert.Equal(358, firstResult.ColumnA.Reference.Id);
                Assert.Equal("BHP", firstResult.ColumnA.Data.Bar);
                Assert.Equal("1", firstResult.ColumnA.Data.Baz);

                var collectedResults = firstResult.ColumnBFromCollect.ToArray();
                Assert.Equal(2, collectedResults.Count());

                var firstCollectedResult = collectedResults[0];
                Assert.Equal(362, firstCollectedResult.Reference.Id);
                Assert.Equal("Board", firstCollectedResult.Data.OpportunityType);
                Assert.Equal("Foo", firstCollectedResult.Data.Description);

                var secondCollectedResult = collectedResults[1];
                Assert.Equal(359, secondCollectedResult.Reference.Id);
                Assert.Equal("Executive", secondCollectedResult.Data.OpportunityType);
                Assert.Equal("Bar", secondCollectedResult.Data.Description);
            }
        }

        public class FooData
        {
            public string Bar { get; set; }
            public string Baz { get; set; }
            public DateTimeOffset? Date { get; set; }
        }

        public class CollectResult
        {
            public Node<FooData> ColumnA { get; set; }
            public IEnumerable<Node<BarData>> ColumnBFromCollect { get; set; }
        }

        public class BarData
        {
            public string OpportunityType { get; set; }
            public string Description { get; set; }
        }

        public class ResultWithNodeDto
        {
            public Node<FooData> Fooness { get; set; }
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        public class ResultWithNodeDataObjectsDto
        {
            public FooData Fooness { get; set; }
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        public class ResultWithRelationshipDto
        {
            public RelationshipInstance<FooData> Fooness { get; set; }
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        [Fact(Skip = "Doesn't Reflect Current Response from Neo4j")]
        public async Task ShouldDeserializeTableStructureWithNodes()
        {
            // Arrange
            const string queryText = @"
                START x = node($p0)
                MATCH x-[r]->n
                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var cypherQuery = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                },
                CypherResultMode.Projection,
                CypherResultFormat.Rest, "neo4j");

             var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

             using (var testHarness = new RestTestHarness
                 {
                    {
                        MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                        MockResponse.Json(HttpStatusCode.OK, @"{
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
                                'self' : 'http://foo/db/data/node/2',
                                'properties' : 'http://foo/db/data/node/0/properties',
                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                                'extensions' : {
                                },
                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                                }, 'LIKES', 'bar', 44311 ], [ {
                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                                'property' : 'http://foo/db/data/node/0/properties/{key}',
                                'self' : 'http://foo/db/data/node/12',
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
                                'columns' : [ 'Fooness', 'RelationshipType', 'Name', 'UniqueId' ]
                            }")
                    }
                })
             {
                 var graphClient = await testHarness.CreateAndConnectGraphClient();

                 //Act
                 var results = await graphClient.ExecuteGetCypherResultsAsync<ResultWithNodeDto>(cypherQuery);

                 //Assert
                 Assert.IsAssignableFrom<IEnumerable<ResultWithNodeDto>>(results);

                 var resultsArray = results.ToArray();
                 Assert.Equal(3, resultsArray.Count());

                 var firstResult = resultsArray[0];
                 Assert.Equal(0, firstResult.Fooness.Reference.Id);
                 Assert.Equal("bar", firstResult.Fooness.Data.Bar);
                 Assert.Equal("baz", firstResult.Fooness.Data.Baz);
                 Assert.Equal("HOSTS", firstResult.RelationshipType);
                 Assert.Equal("foo", firstResult.Name);
                 Assert.Equal(44321, firstResult.UniqueId);

                 var secondResult = resultsArray[1];
                 Assert.Equal(2, secondResult.Fooness.Reference.Id);
                 Assert.Equal("bar", secondResult.Fooness.Data.Bar);
                 Assert.Equal("baz", secondResult.Fooness.Data.Baz);
                 Assert.Equal("LIKES", secondResult.RelationshipType);
                 Assert.Equal("bar", secondResult.Name);
                 Assert.Equal(44311, secondResult.UniqueId);

                 var thirdResult = resultsArray[2];
                 Assert.Equal(12, thirdResult.Fooness.Reference.Id);
                 Assert.Equal("bar", thirdResult.Fooness.Data.Bar);
                 Assert.Equal("baz", thirdResult.Fooness.Data.Baz);
                 Assert.Equal("HOSTS", thirdResult.RelationshipType);
                 Assert.Equal("baz", thirdResult.Name);
                 Assert.Equal(42586, thirdResult.UniqueId);
             }
        }

        [Fact(Skip = "Doesn't Reflect Current Response from Neo4j")]
        public async Task ShouldDeserializeTableStructureWithNodeDataObjects()
        {
            // Arrange
            const string queryText = @"
                START x = node($p0)
                MATCH x-[r]->n
                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var cypherQuery = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                    {
                        {"p0", 123}
                    },
                CypherResultMode.Projection,
                CypherResultFormat.Rest, "neo4j");

            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                        MockResponse.Json(HttpStatusCode.OK, @"{
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
                                'self' : 'http://foo/db/data/node/2',
                                'properties' : 'http://foo/db/data/node/0/properties',
                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
                                'extensions' : {
                                },
                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
                                }, 'LIKES', 'bar', 44311 ], [ {
                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
                                'property' : 'http://foo/db/data/node/0/properties/{key}',
                                'self' : 'http://foo/db/data/node/12',
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
                                'columns' : [ 'Fooness', 'RelationshipType', 'Name', 'UniqueId' ]
                            }")
                    }
                })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                //Act
                var results = await graphClient.ExecuteGetCypherResultsAsync<ResultWithNodeDataObjectsDto>(cypherQuery);

                //Assert
                Assert.IsAssignableFrom<IEnumerable<ResultWithNodeDataObjectsDto>>(results);

                var resultsArray = results.ToArray();
                Assert.Equal(3, resultsArray.Count());

                var firstResult = resultsArray[0];
                Assert.Equal("bar", firstResult.Fooness.Bar);
                Assert.Equal("baz", firstResult.Fooness.Baz);
                Assert.Equal("HOSTS", firstResult.RelationshipType);
                Assert.Equal("foo", firstResult.Name);
                Assert.Equal(44321, firstResult.UniqueId);

                var secondResult = resultsArray[1];
                Assert.Equal("bar", secondResult.Fooness.Bar);
                Assert.Equal("baz", secondResult.Fooness.Baz);
                Assert.Equal("LIKES", secondResult.RelationshipType);
                Assert.Equal("bar", secondResult.Name);
                Assert.Equal(44311, secondResult.UniqueId);

                var thirdResult = resultsArray[2];
                Assert.Equal("bar", thirdResult.Fooness.Bar);
                Assert.Equal("baz", thirdResult.Fooness.Baz);
                Assert.Equal("HOSTS", thirdResult.RelationshipType);
                Assert.Equal("baz", thirdResult.Name);
                Assert.Equal(42586, thirdResult.UniqueId);
            }
        }

        [Fact(Skip = "Doesn't Reflect Current Response from Neo4j")]
        public async Task ShouldDeserializeTableStructureWithRelationships()
        {
            // Arrange
            const string queryText = @"
                START x = node($p0)
                MATCH x-[r]->n
                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var cypherQuery = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                },
                CypherResultMode.Projection,
                CypherResultFormat.Rest, "neo4j");

            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                        MockResponse.Json(HttpStatusCode.OK, @"{
                                'data' : [ [ {
                                'start' : 'http://foo/db/data/node/0',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'property' : 'http://foo/db/data/relationship/0/properties/{key}',
                                'self' : 'http://foo/db/data/relationship/0',
                                'properties' : 'http://foo/db/data/relationship/0/properties',
                                'type' : 'HAS_REFERENCE_DATA',
                                'extensions' : {
                                },
                                'end' : 'http://foo/db/data/node/1'
                                }, 'HOSTS', 'foo', 44321 ], [ {
                                'start' : 'http://foo/db/data/node/1',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'property' : 'http://foo/db/data/relationship/1/properties/{key}',
                                'self' : 'http://foo/db/data/relationship/1',
                                'properties' : 'http://foo/db/data/relationship/1/properties',
                                'type' : 'HAS_REFERENCE_DATA',
                                'extensions' : {
                                },
                                'end' : 'http://foo/db/data/node/1'
                                }, 'LIKES', 'bar', 44311 ], [ {
                                'start' : 'http://foo/db/data/node/2',
                                'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                                },
                                'property' : 'http://foo/db/data/relationship/2/properties/{key}',
                                'self' : 'http://foo/db/data/relationship/2',
                                'properties' : 'http://foo/db/data/relationship/2/properties',
                                'type' : 'HAS_REFERENCE_DATA',
                                'extensions' : {
                                },
                                'end' : 'http://foo/db/data/node/1'
                                }, 'HOSTS', 'baz', 42586 ] ],
                                'columns' : [ 'Fooness', 'RelationshipType', 'Name', 'UniqueId' ]
                            }")
                    }
                })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                //Act
                //Act
                var results = await graphClient.ExecuteGetCypherResultsAsync<ResultWithRelationshipDto>(cypherQuery);

                //Assert
                Assert.IsAssignableFrom<IEnumerable<ResultWithRelationshipDto>>(results);

                var resultsArray = results.ToArray();
                Assert.Equal(3, resultsArray.Count());

                var firstResult = resultsArray[0];
                Assert.Equal(0, firstResult.Fooness.Reference.Id);
                Assert.Equal("bar", firstResult.Fooness.Data.Bar);
                Assert.Equal("baz", firstResult.Fooness.Data.Baz);
                Assert.Equal("HOSTS", firstResult.RelationshipType);
                Assert.Equal("foo", firstResult.Name);
                Assert.Equal(44321, firstResult.UniqueId);

                var secondResult = resultsArray[1];
                Assert.Equal(1, secondResult.Fooness.Reference.Id);
                Assert.Equal("bar", secondResult.Fooness.Data.Bar);
                Assert.Equal("baz", secondResult.Fooness.Data.Baz);
                Assert.Equal("LIKES", secondResult.RelationshipType);
                Assert.Equal("bar", secondResult.Name);
                Assert.Equal(44311, secondResult.UniqueId);

                var thirdResult = resultsArray[2];
                Assert.Equal(2, thirdResult.Fooness.Reference.Id);
                Assert.Equal("bar", thirdResult.Fooness.Data.Bar);
                Assert.Equal("baz", thirdResult.Fooness.Data.Baz);
                Assert.Equal("HOSTS", thirdResult.RelationshipType);
                Assert.Equal("baz", thirdResult.Name);
                Assert.Equal(42586, thirdResult.UniqueId);
            }
        }

        [Fact]
        public async Task ShouldPromoteBadQueryResponseToNiceException()
        {
            // Arrange
            const string queryText = @"broken query";
            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Rest, "neo4j");
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                        MockResponse.Json(HttpStatusCode.BadRequest, @"{
  'message' : 'expected START or CREATE\n\'bad query\'\n ^',
  'exception' : 'SyntaxException',
  'fullname' : 'org.neo4j.cypher.SyntaxException',
  'stacktrace' : [ 'org.neo4j.cypher.internal.parser.v1_9.CypherParserImpl.parse(CypherParserImpl.scala:45)', 'org.neo4j.cypher.CypherParser.parse(CypherParser.scala:44)', 'org.neo4j.cypher.ExecutionEngine$$anonfun$prepare$1.apply(ExecutionEngine.scala:80)', 'org.neo4j.cypher.ExecutionEngine$$anonfun$prepare$1.apply(ExecutionEngine.scala:80)', 'org.neo4j.cypher.internal.LRUCache.getOrElseUpdate(LRUCache.scala:37)', 'org.neo4j.cypher.ExecutionEngine.prepare(ExecutionEngine.scala:80)', 'org.neo4j.cypher.ExecutionEngine.execute(ExecutionEngine.scala:72)', 'org.neo4j.cypher.ExecutionEngine.execute(ExecutionEngine.scala:76)', 'org.neo4j.cypher.javacompat.ExecutionEngine.execute(ExecutionEngine.java:79)', 'org.neo4j.server.rest.web.CypherService.cypher(CypherService.java:94)', 'java.lang.reflect.Method.invoke(Unknown Source)', 'org.neo4j.server.rest.security.SecurityFilter.doFilter(SecurityFilter.java:112)' ]
}")
                    }
                })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                var ex = await Assert.ThrowsAsync<NeoException>(async () => await graphClient.ExecuteGetCypherResultsAsync<ResultWithRelationshipDto>(cypherQuery));
                Assert.Equal("SyntaxException: expected START or CREATE\n'bad query'\n ^", ex.Message);
                Assert.Equal("expected START or CREATE\n'bad query'\n ^", ex.NeoMessage);
                Assert.Equal("SyntaxException", ex.NeoExceptionName);
                Assert.Equal("org.neo4j.cypher.SyntaxException", ex.NeoFullName);

                var expectedStack = new[]
                {
                    "org.neo4j.cypher.internal.parser.v1_9.CypherParserImpl.parse(CypherParserImpl.scala:45)",
                    "org.neo4j.cypher.CypherParser.parse(CypherParser.scala:44)",
                    "org.neo4j.cypher.ExecutionEngine$$anonfun$prepare$1.apply(ExecutionEngine.scala:80)",
                    "org.neo4j.cypher.ExecutionEngine$$anonfun$prepare$1.apply(ExecutionEngine.scala:80)",
                    "org.neo4j.cypher.internal.LRUCache.getOrElseUpdate(LRUCache.scala:37)",
                    "org.neo4j.cypher.ExecutionEngine.prepare(ExecutionEngine.scala:80)",
                    "org.neo4j.cypher.ExecutionEngine.execute(ExecutionEngine.scala:72)",
                    "org.neo4j.cypher.ExecutionEngine.execute(ExecutionEngine.scala:76)",
                    "org.neo4j.cypher.javacompat.ExecutionEngine.execute(ExecutionEngine.java:79)",
                    "org.neo4j.server.rest.web.CypherService.cypher(CypherService.java:94)",
                    "java.lang.reflect.Method.invoke(Unknown Source)",
                    "org.neo4j.server.rest.security.SecurityFilter.doFilter(SecurityFilter.java:112)"
                };
                Assert.Equal(expectedStack, ex.NeoStackTrace);
            }
        }

        [Fact(Skip = "Doesn't Reflect Current Response from Neo4j")]
        public async Task SendsCommandWithCorrectTimeout()
        {
            const int expectedMaxExecutionTime = 100;

            const string queryText = @"START d=node($p0), e=node($p1)
                                        MATCH p = allShortestPaths( d-[*..15]-e )
                                        RETURN p";

            var parameters = new Dictionary<string, object>
                {
                    {"p0", 215},
                    {"p1", 219}
                };


            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set,CypherResultFormat.Transactional, "neo4j", maxExecutionTime: expectedMaxExecutionTime);
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot20()
                },
                {
                    MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                    MockResponse.Json(HttpStatusCode.OK,
                    @"{
                              'data' : [ [ {
                                'start' : 'http://foo/db/data/node/215',
                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/0', 'http://foo/db/data/node/219' ],
                                'length' : 2,
                                'relationships' : [ 'http://foo/db/data/relationship/247', 'http://foo/db/data/relationship/257' ],
                                'end' : 'http://foo/db/data/node/219'
                              } ], [ {
                                'start' : 'http://foo/db/data/node/215',
                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/1', 'http://foo/db/data/node/219' ],
                                'length' : 2,
                                'relationships' : [ 'http://foo/db/data/relationship/248', 'http://foo/db/data/relationship/258' ],
                                'end' : 'http://foo/db/data/node/219'
                              } ] ],
                              'columns' : [ 'p' ]
                            }")
                }
            })
            {
                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);
                var graphClient = new GraphClient(new Uri(testHarness.BaseUri), httpClient);
                await graphClient.ConnectAsync();

                httpClient.ClearReceivedCalls();
                await ((IRawGraphClient)graphClient).ExecuteGetCypherResultsAsync<object>(cypherQuery);

                var call = httpClient.ReceivedCalls().Single();
                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
                var maxExecutionTimeHeader = requestMessage.Headers.Single(h => h.Key == "max-execution-time");
                Assert.Equal(expectedMaxExecutionTime.ToString(CultureInfo.InvariantCulture), maxExecutionTimeHeader.Value.Single());
            }
        }

        [Fact (Skip="Doesn't Reflect Current Response from Neo4j")]
        public async Task DoesntSendMaxExecutionTime_WhenNotAddedToQuery()
        {
            const string queryText = @"START d=node($p0), e=node($p1)
                                        MATCH p = allShortestPaths( d-[*..15]-e )
                                        RETURN p";

            var parameters = new Dictionary<string, object>
                {
                    {"p0", 215},
                    {"p1", 219}
                };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set, "neo4j");
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot20()
                },
                {
                    MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery),
                    MockResponse.Json(HttpStatusCode.OK,
                    @"{
                              'data' : [ [ {
                                'start' : 'http://foo/db/data/node/215',
                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/0', 'http://foo/db/data/node/219' ],
                                'length' : 2,
                                'relationships' : [ 'http://foo/db/data/relationship/247', 'http://foo/db/data/relationship/257' ],
                                'end' : 'http://foo/db/data/node/219'
                              } ], [ {
                                'start' : 'http://foo/db/data/node/215',
                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/1', 'http://foo/db/data/node/219' ],
                                'length' : 2,
                                'relationships' : [ 'http://foo/db/data/relationship/248', 'http://foo/db/data/relationship/258' ],
                                'end' : 'http://foo/db/data/node/219'
                              } ] ],
                              'columns' : [ 'p' ]
                            }")
                }
            })
            {
                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);
                var graphClient = new GraphClient(new Uri(testHarness.BaseUri), httpClient);
                await graphClient.ConnectAsync();

                httpClient.ClearReceivedCalls();
                await ((IRawGraphClient)graphClient).ExecuteGetCypherResultsAsync<object>(cypherQuery);

                var call = httpClient.ReceivedCalls().Single();
                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
                Assert.False(requestMessage.Headers.Any(h => h.Key == "max-execution-time"));
            }
        }


    }
}
