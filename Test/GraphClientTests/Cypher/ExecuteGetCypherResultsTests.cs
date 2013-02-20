using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class ExecuteGetCypherResultsTests
    {
        public class SimpleResultDto
        {
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        [Test] public void ShouldDeserializePathsResultAsSetBased()
        {
            // Arrange
            const string queryText = @"START d=node({p0}), e=node({p1})
                                        MATCH p = allShortestPaths( d-[*..15]-e )
                                        RETURN p";

            var parameters = new Dictionary<string, object>
                {
                    {"p0", 215},
                    {"p1", 219}
                };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set);
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
                {
                    {
                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
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
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var results = graphClient
                    .ExecuteGetCypherResults<PathsResult>(cypherQuery)
                    .ToArray();

                //Assert
                Assert.IsInstanceOf<IEnumerable<PathsResult>>(results);
                Assert.AreEqual(results.First().Length, 2);
                Assert.AreEqual(results.First().Start, "http://foo/db/data/node/215");
                Assert.AreEqual(results.First().End, "http://foo/db/data/node/219");
                Assert.AreEqual(results.Skip(1).First().Length, 2);
                Assert.AreEqual(results.Skip(1).First().Start, "http://foo/db/data/node/215");
                Assert.AreEqual(results.Skip(1).First().End, "http://foo/db/data/node/219");
            }
        }

        [Test]
        public void ShouldDeserializeSimpleTableStructure()
        {
            // Arrange
            const string queryText = @"
                START x = node({p0})
                MATCH x-[r]->n
                RETURN type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var cypherQuery = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                },
                CypherResultMode.Projection);

            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using(var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
                        MockResponse.Json(HttpStatusCode.OK, @"{
                                'data' : [ [ 'HOSTS', 'foo', 44321 ], [ 'LIKES', 'bar', 44311 ], [ 'HOSTS', 'baz', 42586 ] ],
                                'columns' : [ 'RelationshipType', 'Name', 'UniqueId' ]
                            }")
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var results = graphClient.ExecuteGetCypherResults<SimpleResultDto>(cypherQuery);

                //Assert
                Assert.IsInstanceOf<IEnumerable<SimpleResultDto>>(results);

                var resultsArray = results.ToArray();
                Assert.AreEqual(3, resultsArray.Count());

                var firstResult = resultsArray[0];
                Assert.AreEqual("HOSTS", firstResult.RelationshipType);
                Assert.AreEqual("foo", firstResult.Name);
                Assert.AreEqual(44321, firstResult.UniqueId);

                var secondResult = resultsArray[1];
                Assert.AreEqual("LIKES", secondResult.RelationshipType);
                Assert.AreEqual("bar", secondResult.Name);
                Assert.AreEqual(44311, secondResult.UniqueId);

                var thirdResult = resultsArray[2];
                Assert.AreEqual("HOSTS", thirdResult.RelationshipType);
                Assert.AreEqual("baz", thirdResult.Name);
                Assert.AreEqual(42586, thirdResult.UniqueId);
            }
        }

        [Test]
        public void ShouldDeserializeArrayOfNodesInPropertyAsResultOfCollectFunctionInCypherQuery()
        {
            // Arrange
            var cypherQuery = new CypherQuery(
                @"START root=node(0) MATCH root-[:HAS_COMPANIES]->()-[:HAS_COMPANY]->company, company--foo RETURN company, collect(foo) as Bar",
                new Dictionary<string, object>(),
                CypherResultMode.Projection);

            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
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
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var results = graphClient.ExecuteGetCypherResults<CollectResult>(cypherQuery);

                //Assert
                Assert.IsInstanceOf<IEnumerable<CollectResult>>(results);

                var resultsArray = results.ToArray();
                Assert.AreEqual(1, resultsArray.Count());

                var firstResult = resultsArray[0];
                Assert.AreEqual(358, firstResult.ColumnA.Reference.Id);
                Assert.AreEqual("BHP", firstResult.ColumnA.Data.Bar);
                Assert.AreEqual("1", firstResult.ColumnA.Data.Baz);

                var collectedResults = firstResult.ColumnBFromCollect.ToArray();
                Assert.AreEqual(2, collectedResults.Count());

                var firstCollectedResult = collectedResults[0];
                Assert.AreEqual(362, firstCollectedResult.Reference.Id);
                Assert.AreEqual("Board", firstCollectedResult.Data.OpportunityType);
                Assert.AreEqual("Foo", firstCollectedResult.Data.Description);

                var secondCollectedResult = collectedResults[1];
                Assert.AreEqual(359, secondCollectedResult.Reference.Id);
                Assert.AreEqual("Executive", secondCollectedResult.Data.OpportunityType);
                Assert.AreEqual("Bar", secondCollectedResult.Data.Description);
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

        [Test]
        public void ShouldDeserializeTableStructureWithNodes()
        {
            // Arrange
            const string queryText = @"
                START x = node({p0})
                MATCH x-[r]->n
                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var cypherQuery = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                },
                CypherResultMode.Projection);

             var cypherApiQuery = new CypherApiQuery(cypherQuery);

             using (var testHarness = new RestTestHarness
                 {
                    {
                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
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
                 var graphClient = testHarness.CreateAndConnectGraphClient();

                 //Act
                 var results = graphClient.ExecuteGetCypherResults<ResultWithNodeDto>(cypherQuery);

                 //Assert
                 Assert.IsInstanceOf<IEnumerable<ResultWithNodeDto>>(results);

                 var resultsArray = results.ToArray();
                 Assert.AreEqual(3, resultsArray.Count());

                 var firstResult = resultsArray[0];
                 Assert.AreEqual(0, firstResult.Fooness.Reference.Id);
                 Assert.AreEqual("bar", firstResult.Fooness.Data.Bar);
                 Assert.AreEqual("baz", firstResult.Fooness.Data.Baz);
                 Assert.AreEqual("HOSTS", firstResult.RelationshipType);
                 Assert.AreEqual("foo", firstResult.Name);
                 Assert.AreEqual(44321, firstResult.UniqueId);

                 var secondResult = resultsArray[1];
                 Assert.AreEqual(2, secondResult.Fooness.Reference.Id);
                 Assert.AreEqual("bar", secondResult.Fooness.Data.Bar);
                 Assert.AreEqual("baz", secondResult.Fooness.Data.Baz);
                 Assert.AreEqual("LIKES", secondResult.RelationshipType);
                 Assert.AreEqual("bar", secondResult.Name);
                 Assert.AreEqual(44311, secondResult.UniqueId);

                 var thirdResult = resultsArray[2];
                 Assert.AreEqual(12, thirdResult.Fooness.Reference.Id);
                 Assert.AreEqual("bar", thirdResult.Fooness.Data.Bar);
                 Assert.AreEqual("baz", thirdResult.Fooness.Data.Baz);
                 Assert.AreEqual("HOSTS", thirdResult.RelationshipType);
                 Assert.AreEqual("baz", thirdResult.Name);
                 Assert.AreEqual(42586, thirdResult.UniqueId);
             }
        }

        [Test]
        public void ShouldDeserializeTableStructureWithNodeDataObjects()
        {
            // Arrange
            const string queryText = @"
                START x = node({p0})
                MATCH x-[r]->n
                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var cypherQuery = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                    {
                        {"p0", 123}
                    },
                CypherResultMode.Projection);

            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
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
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var results = graphClient.ExecuteGetCypherResults<ResultWithNodeDataObjectsDto>(cypherQuery);

                //Assert
                Assert.IsInstanceOf<IEnumerable<ResultWithNodeDataObjectsDto>>(results);

                var resultsArray = results.ToArray();
                Assert.AreEqual(3, resultsArray.Count());

                var firstResult = resultsArray[0];
                Assert.AreEqual("bar", firstResult.Fooness.Bar);
                Assert.AreEqual("baz", firstResult.Fooness.Baz);
                Assert.AreEqual("HOSTS", firstResult.RelationshipType);
                Assert.AreEqual("foo", firstResult.Name);
                Assert.AreEqual(44321, firstResult.UniqueId);

                var secondResult = resultsArray[1];
                Assert.AreEqual("bar", secondResult.Fooness.Bar);
                Assert.AreEqual("baz", secondResult.Fooness.Baz);
                Assert.AreEqual("LIKES", secondResult.RelationshipType);
                Assert.AreEqual("bar", secondResult.Name);
                Assert.AreEqual(44311, secondResult.UniqueId);

                var thirdResult = resultsArray[2];
                Assert.AreEqual("bar", thirdResult.Fooness.Bar);
                Assert.AreEqual("baz", thirdResult.Fooness.Baz);
                Assert.AreEqual("HOSTS", thirdResult.RelationshipType);
                Assert.AreEqual("baz", thirdResult.Name);
                Assert.AreEqual(42586, thirdResult.UniqueId);
            }
        }

        [Test]
        public void ShouldDeserializeTableStructureWithRelationships()
        {
            // Arrange
            const string queryText = @"
                START x = node({p0})
                MATCH x-[r]->n
                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var cypherQuery = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                },
                CypherResultMode.Projection);

            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
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
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                //Act
                var results = graphClient.ExecuteGetCypherResults<ResultWithRelationshipDto>(cypherQuery);

                //Assert
                Assert.IsInstanceOf<IEnumerable<ResultWithRelationshipDto>>(results);

                var resultsArray = results.ToArray();
                Assert.AreEqual(3, resultsArray.Count());

                var firstResult = resultsArray[0];
                Assert.AreEqual(0, firstResult.Fooness.Reference.Id);
                Assert.AreEqual("bar", firstResult.Fooness.Data.Bar);
                Assert.AreEqual("baz", firstResult.Fooness.Data.Baz);
                Assert.AreEqual("HOSTS", firstResult.RelationshipType);
                Assert.AreEqual("foo", firstResult.Name);
                Assert.AreEqual(44321, firstResult.UniqueId);

                var secondResult = resultsArray[1];
                Assert.AreEqual(1, secondResult.Fooness.Reference.Id);
                Assert.AreEqual("bar", secondResult.Fooness.Data.Bar);
                Assert.AreEqual("baz", secondResult.Fooness.Data.Baz);
                Assert.AreEqual("LIKES", secondResult.RelationshipType);
                Assert.AreEqual("bar", secondResult.Name);
                Assert.AreEqual(44311, secondResult.UniqueId);

                var thirdResult = resultsArray[2];
                Assert.AreEqual(2, thirdResult.Fooness.Reference.Id);
                Assert.AreEqual("bar", thirdResult.Fooness.Data.Bar);
                Assert.AreEqual("baz", thirdResult.Fooness.Data.Baz);
                Assert.AreEqual("HOSTS", thirdResult.RelationshipType);
                Assert.AreEqual("baz", thirdResult.Name);
                Assert.AreEqual(42586, thirdResult.UniqueId);
            }
        }
    }
}
