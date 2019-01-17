using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Neo4j.Driver.V1;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.BoltGraphClientTests.Cypher
{
    internal class TestPath : IPath
    {
        #region Implementation of IEquatable<IPath>

        public bool Equals(IPath other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IPath

        public INode Start { get; set; }
        public INode End { get; set; }
        public IReadOnlyList<INode> Nodes { get; set; }
        public IReadOnlyList<IRelationship> Relationships { get; set; }

        #endregion
    }

    public class TestRelationship : IRelationship
    {
        #region Implementation of IEntity

        public object this[string key]
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyDictionary<string, object> Properties { get; set; }
        public long Id { get; set; }

        #endregion

        #region Implementation of IEquatable<IRelationship>

        public bool Equals(IRelationship other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IRelationship

        public string Type { get; set; }
        public long StartNodeId { get; set; }
        public long EndNodeId { get; set; }

        #endregion
    }

    public class TestNode : INode {
        #region Implementation of IEntity

        public object this[string key]
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyDictionary<string, object> Properties { get; set; }
        public long Id { get; set; }

        #endregion

        #region Implementation of IEquatable<INode>

        public bool Equals(INode other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of INode

        public IReadOnlyList<string> Labels { get; }

        #endregion
    }

    public class ExecuteGetCypherResultsTests : IClassFixture<CultureInfoSetupFixture>
    {
        public class SimpleResultDto
        {
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        private class ObjectWithId
        {
            public long Id { get; set; }
        }

        private class ObjectWithIds
        {
            public List<int> Ids { get; set; }
        }

        private class RelationType
        {
            public int Id { get; set; }
        }

        private class RelationGrouper
        {
            public RelationType Rel { get; set; }
        }

        [Fact]
        public void RelationshipShouldDeserializeInDefinedType()
        {
            // Arrange
            const string queryText = "MATCH (n:Test)-[r]->(t:Test) RETURN r AS Rel";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                var relationshipMock = new Mock<IRelationship>();
                relationshipMock
                    .Setup(r => r.StartNodeId)
                    .Returns(1);
                relationshipMock
                    .Setup(r => r.EndNodeId)
                    .Returns(2);
                relationshipMock
                    .Setup(r => r.Type)
                    .Returns("Xx");
                relationshipMock
                    .Setup(r => r.Id)
                    .Returns(3);
                relationshipMock
                    .Setup(r => r.Properties)
                    .Returns(new Dictionary<string, object>()
                    {
                        {"Id", 42}
                    });

                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["Rel"])
                    .Returns(relationshipMock.Object);
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "Rel" });

                var testStatementResult = new TestStatementResult(new[] { "Rel" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var results = graphClient.ExecuteGetCypherResults<RelationGrouper>(cypherQuery).ToArray();

                //Assert
                results.Length.Should().Be(1);
                var relation = results.First().Rel;
                relation.Id.Should().Be(42);
            }
        }

        [Fact]
        public void RelationshipShouldDeserializeInAnonymousType()
        {
            // Arrange
            const string queryText = @"MATCH (n:Test)-[r]->(t:Test) RETURN r AS Rel";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                var relationshipMock = new Mock<IRelationship>();
                relationshipMock
                    .Setup(r => r.StartNodeId)
                    .Returns(1);
                relationshipMock
                    .Setup(r => r.EndNodeId)
                    .Returns(2);
                relationshipMock
                    .Setup(r => r.Type)
                    .Returns("Xx");
                relationshipMock
                    .Setup(r => r.Id)
                    .Returns(3);
                relationshipMock
                    .Setup(r => r.Properties)
                    .Returns(new Dictionary<string, object>()
                    {
                        {"Id", 42}
                    });

                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["Rel"])
                    .Returns(relationshipMock.Object);
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "Rel" });

                var testStatementResult = new TestStatementResult(new[] { "Rel" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                //Session mock???   
                var dummy = new
                {
                    Rel = new RelationType()
                };
                var anonType = dummy.GetType();
                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var genericGetCypherResults = typeof(IRawGraphClient).GetMethod(nameof(graphClient.ExecuteGetCypherResults));
                var anonymousGetCypherResults = genericGetCypherResults.MakeGenericMethod(anonType);
                var genericResults = (IEnumerable)anonymousGetCypherResults.Invoke(graphClient, new object[] { cypherQuery });

                var results = genericResults.Cast<object>().ToArray();

                //Assert
                Assert.Equal(1, results.Length);
                var relation = (RelationType)anonType.GetProperty(nameof(dummy.Rel)).GetValue(results.First(), null);
                relation.Id.Should().Be(42);
            }
        }

        [Fact]
        public void CollectionShouldDeserializeCorrectly()
        {
            // simulate a collect()
            const string queryText = "MATCH (start:Node) RETURN collect(start) AS data";

            var queryParams = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                var nodeMock = new Mock<INode>();
                nodeMock
                    .Setup(n => n.Id)
                    .Returns(1);
                nodeMock
                    .Setup(n => n.Properties)
                    .Returns(new Dictionary<string, object>() {{"Ids", new List<int> {1, 2, 3}}});

                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["data"])
                    .Returns(new List<INode>() {nodeMock.Object});
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "data" });

                var testStatementResult = new TestStatementResult(new[] { "data" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var results = graphClient.ExecuteGetCypherResults<IEnumerable<ObjectWithIds>>(cypherQuery).ToArray();

                //Assert
                var deserializedObject = results.First().First();
                deserializedObject.Ids.Count.Should().Be(3);
                deserializedObject.Ids[0].Should().Be(1);
                deserializedObject.Ids[1].Should().Be(2);
                deserializedObject.Ids[2].Should().Be(3);
            }
        }

        [Fact]
        public void EmptyCollectionShouldDeserializeCorrectly()
        {
            const string queryText = "RETURN [] AS data";

            var queryParams = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["data"])
                    .Returns(new List<INode>() {});
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "data" });

                var testStatementResult = new TestStatementResult(new[] { "data" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var results = graphClient.ExecuteGetCypherResults<IEnumerable<ObjectWithIds>>(cypherQuery).ToArray();

                results.Should().BeEmpty();
            }
        }

        //https://github.com/readify/neo4jclient/issues/266
        [Fact]
        public void CollectionOfComplexTypesShouldDeserializeCorrectlyWhenInConjunctionWithAnotherComplexTypeInAContainer()
        {
            const string queryText = "MATCH (start:Node)-->(next:Node) RETURN start AS Start, collect(next) AS Next";

            var queryParams = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Projection, CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                var startNodeMock = new Mock<INode>();
                startNodeMock
                    .Setup(n => n.Id)
                    .Returns(1);
                startNodeMock
                    .Setup(n => n.Properties)
                    .Returns(new Dictionary<string, object> { { "Id", 1 } });

                var nextNodeMock = new Mock<INode>();
                nextNodeMock
                    .Setup(n => n.Id)
                    .Returns(2);
                nextNodeMock
                    .Setup(n => n.Properties)
                    .Returns(new Dictionary<string, object> { { "Id", 2 } });

                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["Next"])
                    .Returns(new List<INode> { nextNodeMock.Object });

                recordMock
                    .Setup(r => r["Start"])
                    .Returns(startNodeMock.Object);

                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "Start", "Next" });

                var testStatementResult = new TestStatementResult(new[] { "Start", "Next" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var results = graphClient.ExecuteGetCypherResults<Container>(cypherQuery).ToArray();

                //Assert
                var deserializedObject = results.First();
                deserializedObject.Start.Should().NotBeNull();
                deserializedObject.Start.Id.Should().Be(1);

                var deserializedNext = deserializedObject.Next.ToList();
                deserializedNext.Should().HaveCount(1);
                deserializedNext.First().Id.Should().Be(2);
            }
        }

        private class Container
        {
            public ObjectWithId Start { get; set; }
            public IEnumerable<ObjectWithId> Next { get; set; }
        }

        [Fact]
        public void CreateWithArrayParametersShouldSerializeAndDeserializeOnReturn()
        {
            // Arrange
            const string queryText = "CREATE (start:Node {obj}) RETURN start";

            var testNode = new ObjectWithIds()
            {
                Ids = new List<int>() {1, 2, 3}
            };

            var queryParams = new Dictionary<string, object>() {{"obj", testNode}};

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["start"])
                    .Returns(testNode);
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "start" });

                var testStatementResult = new TestStatementResult(new[] { "start" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var results = graphClient.ExecuteGetCypherResults<ObjectWithIds>(cypherQuery).ToArray();

                //Assert
                Assert.IsAssignableFrom<IEnumerable<ObjectWithIds>>(results);
                results.First().Ids.Count.Should().Be(3);
                results.First().Ids[0].Should().Be(1);
                results.First().Ids[1].Should().Be(2);
                results.First().Ids[2].Should().Be(3);
            }
        }

        private class ObjectWithNodeWithIds
        {
            public ObjectWithIds Node { get; set; }
            public int Count { get; set; }
        }

        [Fact]
        public void ShouldDeserializeMapWithAnonymousReturnAsDictionary()
        {
            // simulates the following query
            const string queryText = "MATCH (start:Node) WITH {Node: 3, Count: 1} AS Node, start RETURN Node, start";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection,
                CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                INode start = new TestNode
                {
                    Id = 1337,
                    Properties = new Dictionary<string, object>()
                    {
                        {"Ids", new List<int>() {1, 2, 3}}
                    }
                };
                IDictionary<string, object> resultMap = new Dictionary<string, object>()
                {
                    {"Node", 3},
                    {"Count", 1}
                };

                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["Node"])
                    .Returns(resultMap);
                recordMock
                    .Setup(r => r["Start"])
                    .Returns(start);
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "Node", "Start" });

                var testStatementResult = new TestStatementResult(new[] { "Node", "Start" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                // the anon type
                var dummy = new
                {
                    Node = new Dictionary<string, int>(),
                    Start = new ObjectWithIds()
                };
                var anonType = dummy.GetType();
                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var genericGetCypherResults = typeof(IRawGraphClient).GetMethod(nameof(graphClient.ExecuteGetCypherResults));
                var anonymousGetCypherResults = genericGetCypherResults.MakeGenericMethod(anonType);
                var genericResults = (IEnumerable)anonymousGetCypherResults.Invoke(graphClient, new object[] { cypherQuery });

                var results = genericResults.Cast<object>().ToArray();

                //Assert
                Assert.Equal(1, results.Length);

                var startNode = (ObjectWithIds)anonType.GetProperty(nameof(dummy.Start)).GetValue(results.First(), null);
                startNode.Ids.Count.Should().Be(3);
                startNode.Ids[0].Should().Be(1);
                startNode.Ids[1].Should().Be(2);
                startNode.Ids[2].Should().Be(3);

                var nodeWrapper = (Dictionary<string, int>)anonType.GetProperty(nameof(dummy.Node)).GetValue(results.First(), null);
                nodeWrapper.Keys.Count.Should().Be(2);
                nodeWrapper["Node"].Should().Be(3);
                nodeWrapper["Count"].Should().Be(1);
            }
        }

        [Fact]
        public void ShouldDeserializeMapWithAnonymousReturn()
        {
            // simulates the following query
            const string queryText = "MATCH (start:Node) WITH {Node: start, Count: 1} AS Node, start RETURN Node, start";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection,
                CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                INode start = new TestNode
                {
                    Id = 1337,
                    Properties = new Dictionary<string, object>()
                    {
                        {"Ids", new List<int>() {1, 2, 3}}
                    }
                };
                IDictionary<string, object> resultMap = new Dictionary<string, object>()
                {
                    {"Node", start},
                    {"Count", 1}
                };

                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["Node"])
                    .Returns(resultMap);
                recordMock
                    .Setup(r => r["Start"])
                    .Returns(start);
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "Node", "Start" });

                var testStatementResult = new TestStatementResult(new[] { "Node", "Start" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                // the anon type
                var dummy = new
                {
                    Node = new ObjectWithNodeWithIds(),
                    Start = new ObjectWithIds()
                };
                var anonType = dummy.GetType();
                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var genericGetCypherResults = typeof(IRawGraphClient).GetMethod(nameof(graphClient.ExecuteGetCypherResults));
                var anonymousGetCypherResults = genericGetCypherResults.MakeGenericMethod(anonType);
                var genericResults = (IEnumerable)anonymousGetCypherResults.Invoke(graphClient, new object[] { cypherQuery });

                var results = genericResults.Cast<object>().ToArray();

                //Assert
                Assert.Equal(1, results.Length);

                var startNode = (ObjectWithIds)anonType.GetProperty(nameof(dummy.Start)).GetValue(results.First(), null);
                startNode.Ids.Count.Should().Be(3);
                startNode.Ids[0].Should().Be(1);
                startNode.Ids[1].Should().Be(2);
                startNode.Ids[2].Should().Be(3);

                var nodeWrapper = (ObjectWithNodeWithIds)anonType.GetProperty(nameof(dummy.Node)).GetValue(results.First(), null);
                nodeWrapper.Count.Should().Be(1);
                startNode = nodeWrapper.Node;

                startNode.Ids.Count.Should().Be(3);
                startNode.Ids[0].Should().Be(1);
                startNode.Ids[1].Should().Be(2);
                startNode.Ids[2].Should().Be(3);
            }

        }

        [Fact]
        public void ShouldDeserializeCollectionsWithAnonymousReturn()
        {
            // Arrange
            const string queryText = @"MATCH (start:Node) RETURN [start.Id, start.Id] AS Ids";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional);

            using (var testHarness = new BoltTestHarness())
            {
                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["Ids"])
                    .Returns(new[] {1, 2, 3});
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "Ids" });

                var testStatementResult = new TestStatementResult(new[] { "Ids" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                //Session mock???
                var dummy = new
                {
                    Ids = new List<int>()
                };
                var anonType = dummy.GetType();
                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var genericGetCypherResults = typeof(IRawGraphClient).GetMethod(nameof(graphClient.ExecuteGetCypherResults));
                var anonymousGetCypherResults = genericGetCypherResults.MakeGenericMethod(anonType);
                var genericResults = (IEnumerable)anonymousGetCypherResults.Invoke(graphClient, new object[] {cypherQuery});

                var results = genericResults.Cast<object>().ToArray();

                //Assert
                Assert.Equal(1, results.Length);
                var ids = (List<int>)anonType.GetProperty(nameof(dummy.Ids)).GetValue(results.First(), null);
                ids.Count.Should().Be(3);
                ids[0].Should().Be(1);
                ids[1].Should().Be(2);
                ids[2].Should().Be(3);
            }
        }

        [Fact]
        public void ShouldDeserializePathsResultAsSetBased()
        {
            // Arrange
            const string queryText = @"MATCH (start:Node {Id:{p0}}),(end:Node {Id: {p1}}), p = shortestPath((start)-[*..5]->(end)) RETURN p";

            var parameters = new Dictionary<string, object>
            {
                {"p0", 215},
                {"p1", 219}
            };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set, CypherResultFormat.Rest);
            
            using (var testHarness = new BoltTestHarness())
            {
                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["p"])
                    .Returns(new TestPath {End = new TestNode{Id = 1}, Start = new TestNode{Id=2}, Relationships = new List<IRelationship> {new TestRelationship()}, Nodes = new List<INode> {new TestNode(), new TestNode() } });
                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] {"p"});
                
                var testStatementResult = new TestStatementResult(new[] {"p"}, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                //Session mock???
                var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                var results = graphClient.ExecuteGetCypherResults<PathsResultBolt>(cypherQuery).ToArray();

                //Assert
                Assert.IsAssignableFrom<IEnumerable<PathsResultBolt>>(results);
                Assert.Equal(1, results.First().Length);
                results.First().Nodes.Count().Should().Be(2);
                results.First().Relationships.Count().Should().Be(1);
                results.First().End.Id.Should().Be(1);
                results.First().Start.Id.Should().Be(2);
            }
        }

        

        //        [Fact]
        //        public void ShouldDeserializeSimpleTableStructure()
        //        {
        //            // Arrange
        //            const string queryText = @"
        //                START x = node({p0})
        //                MATCH x-[r]->n
        //                RETURN type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
        //                LIMIT 3";
        //            var cypherQuery = new CypherQuery(
        //                queryText,
        //                new Dictionary<string, object>
        //                {
        //                    {"p0", 123}
        //                },
        //                CypherResultMode.Projection,
        //                CypherResultFormat.Rest);
        //
        //            var cypherApiQuery = new CypherApiQuery(cypherQuery);
        //
        //            using(var testHarness = new RestTestHarness
        //                {
        //                    {
        //                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
        //                        MockResponse.Json(HttpStatusCode.OK, @"{
        //                                'data' : [ [ 'HOSTS', 'foo', 44321 ], [ 'LIKES', 'bar', 44311 ], [ 'HOSTS', 'baz', 42586 ] ],
        //                                'columns' : [ 'RelationshipType', 'Name', 'UniqueId' ]
        //                            }")
        //                    }
        //                })
        //            {
        //                var graphClient = testHarness.CreateAndConnectGraphClient();
        //
        //                //Act
        //                var results = graphClient.ExecuteGetCypherResults<SimpleResultDto>(cypherQuery);
        //
        //                //Assert
        //                Assert.IsAssignableFrom<IEnumerable<SimpleResultDto>>(results);
        //
        //                var resultsArray = results.ToArray();
        //                Assert.Equal(3, resultsArray.Count());
        //
        //                var firstResult = resultsArray[0];
        //                Assert.Equal("HOSTS", firstResult.RelationshipType);
        //                Assert.Equal("foo", firstResult.Name);
        //                Assert.Equal(44321, firstResult.UniqueId);
        //
        //                var secondResult = resultsArray[1];
        //                Assert.Equal("LIKES", secondResult.RelationshipType);
        //                Assert.Equal("bar", secondResult.Name);
        //                Assert.Equal(44311, secondResult.UniqueId);
        //
        //                var thirdResult = resultsArray[2];
        //                Assert.Equal("HOSTS", thirdResult.RelationshipType);
        //                Assert.Equal("baz", thirdResult.Name);
        //                Assert.Equal(42586, thirdResult.UniqueId);
        //            }
        //        }
        //
        //        [Fact]
        //        public void ShouldDeserializeArrayOfNodesInPropertyAsResultOfCollectFunctionInCypherQuery()
        //        {
        //            // Arrange
        //            var cypherQuery = new CypherQuery(
        //                @"START root=node(0) MATCH root-[:HAS_COMPANIES]->()-[:HAS_COMPANY]->company, company--foo RETURN company, collect(foo) as Bar",
        //                new Dictionary<string, object>(),
        //                CypherResultMode.Projection,
        //                CypherResultFormat.Rest);
        //
        //            var cypherApiQuery = new CypherApiQuery(cypherQuery);
        //
        //            using (var testHarness = new RestTestHarness
        //                {
        //                    {
        //                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
        //                        MockResponse.Json(HttpStatusCode.OK, @"{
        //  'columns' : [ 'ColumnA', 'ColumnBFromCollect' ],
        //  'data' : [ [ {
        //    'paged_traverse' : 'http://localhost:8000/db/data/node/358/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //    'outgoing_relationships' : 'http://localhost:8000/db/data/node/358/relationships/out',
        //    'data' : {
        //      'Bar' : 'BHP',
        //      'Baz' : '1'
        //    },
        //    'all_typed_relationships' : 'http://localhost:8000/db/data/node/358/relationships/all/{-list|&|types}',
        //    'traverse' : 'http://localhost:8000/db/data/node/358/traverse/{returnType}',
        //    'self' : 'http://localhost:8000/db/data/node/358',
        //    'property' : 'http://localhost:8000/db/data/node/358/properties/{key}',
        //    'all_relationships' : 'http://localhost:8000/db/data/node/358/relationships/all',
        //    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/358/relationships/out/{-list|&|types}',
        //    'properties' : 'http://localhost:8000/db/data/node/358/properties',
        //    'incoming_relationships' : 'http://localhost:8000/db/data/node/358/relationships/in',
        //    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/358/relationships/in/{-list|&|types}',
        //    'extensions' : {
        //    },
        //    'create_relationship' : 'http://localhost:8000/db/data/node/358/relationships'
        //  }, [ {
        //    'paged_traverse' : 'http://localhost:8000/db/data/node/362/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //    'outgoing_relationships' : 'http://localhost:8000/db/data/node/362/relationships/out',
        //    'data' : {
        //      'OpportunityType' : 'Board',
        //      'Description' : 'Foo'
        //    },
        //    'all_typed_relationships' : 'http://localhost:8000/db/data/node/362/relationships/all/{-list|&|types}',
        //    'traverse' : 'http://localhost:8000/db/data/node/362/traverse/{returnType}',
        //    'self' : 'http://localhost:8000/db/data/node/362',
        //    'property' : 'http://localhost:8000/db/data/node/362/properties/{key}',
        //    'all_relationships' : 'http://localhost:8000/db/data/node/362/relationships/all',
        //    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/362/relationships/out/{-list|&|types}',
        //    'properties' : 'http://localhost:8000/db/data/node/362/properties',
        //    'incoming_relationships' : 'http://localhost:8000/db/data/node/362/relationships/in',
        //    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/362/relationships/in/{-list|&|types}',
        //    'extensions' : {
        //    },
        //    'create_relationship' : 'http://localhost:8000/db/data/node/362/relationships'
        //  }, {
        //    'paged_traverse' : 'http://localhost:8000/db/data/node/359/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //    'outgoing_relationships' : 'http://localhost:8000/db/data/node/359/relationships/out',
        //    'data' : {
        //      'OpportunityType' : 'Executive',
        //      'Description' : 'Bar'
        //    },
        //    'all_typed_relationships' : 'http://localhost:8000/db/data/node/359/relationships/all/{-list|&|types}',
        //    'traverse' : 'http://localhost:8000/db/data/node/359/traverse/{returnType}',
        //    'self' : 'http://localhost:8000/db/data/node/359',
        //    'property' : 'http://localhost:8000/db/data/node/359/properties/{key}',
        //    'all_relationships' : 'http://localhost:8000/db/data/node/359/relationships/all',
        //    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/359/relationships/out/{-list|&|types}',
        //    'properties' : 'http://localhost:8000/db/data/node/359/properties',
        //    'incoming_relationships' : 'http://localhost:8000/db/data/node/359/relationships/in',
        //    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/359/relationships/in/{-list|&|types}',
        //    'extensions' : {
        //    },
        //    'create_relationship' : 'http://localhost:8000/db/data/node/359/relationships'
        //  } ] ] ]
        //}")
        //                    }
        //                })
        //            {
        //                var graphClient = testHarness.CreateAndConnectGraphClient();
        //
        //                //Act
        //                var results = graphClient.ExecuteGetCypherResults<CollectResult>(cypherQuery);
        //
        //                //Assert
        //                Assert.IsAssignableFrom<IEnumerable<CollectResult>>(results);
        //
        //                var resultsArray = results.ToArray();
        //                Assert.Equal(1, resultsArray.Count());
        //
        //                var firstResult = resultsArray[0];
        //                Assert.Equal(358, firstResult.ColumnA.Reference.Id);
        //                Assert.Equal("BHP", firstResult.ColumnA.Data.Bar);
        //                Assert.Equal("1", firstResult.ColumnA.Data.Baz);
        //
        //                var collectedResults = firstResult.ColumnBFromCollect.ToArray();
        //                Assert.Equal(2, collectedResults.Count());
        //
        //                var firstCollectedResult = collectedResults[0];
        //                Assert.Equal(362, firstCollectedResult.Reference.Id);
        //                Assert.Equal("Board", firstCollectedResult.Data.OpportunityType);
        //                Assert.Equal("Foo", firstCollectedResult.Data.Description);
        //
        //                var secondCollectedResult = collectedResults[1];
        //                Assert.Equal(359, secondCollectedResult.Reference.Id);
        //                Assert.Equal("Executive", secondCollectedResult.Data.OpportunityType);
        //                Assert.Equal("Bar", secondCollectedResult.Data.Description);
        //            }
        //        }
        //
        //        public class FooData
        //        {
        //            public string Bar { get; set; }
        //            public string Baz { get; set; }
        //            public DateTimeOffset? Date { get; set; }
        //        }
        //
        //        public class CollectResult
        //        {
        //            public Node<FooData> ColumnA { get; set; }
        //            public IEnumerable<Node<BarData>> ColumnBFromCollect { get; set; }
        //        }
        //
        //        public class BarData
        //        {
        //            public string OpportunityType { get; set; }
        //            public string Description { get; set; }
        //        }
        //
        //        public class ResultWithNodeDto
        //        {
        //            public Node<FooData> Fooness { get; set; }
        //            public string RelationshipType { get; set; }
        //            public string Name { get; set; }
        //            public long? UniqueId { get; set; }
        //        }
        //
        //        public class ResultWithNodeDataObjectsDto
        //        {
        //            public FooData Fooness { get; set; }
        //            public string RelationshipType { get; set; }
        //            public string Name { get; set; }
        //            public long? UniqueId { get; set; }
        //        }
        //
        //        public class ResultWithRelationshipDto
        //        {
        //            public RelationshipInstance<FooData> Fooness { get; set; }
        //            public string RelationshipType { get; set; }
        //            public string Name { get; set; }
        //            public long? UniqueId { get; set; }
        //        }
        //
        //        [Fact]
        //        public void ShouldDeserializeTableStructureWithNodes()
        //        {
        //            // Arrange
        //            const string queryText = @"
        //                START x = node({p0})
        //                MATCH x-[r]->n
        //                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
        //                LIMIT 3";
        //            var cypherQuery = new CypherQuery(
        //                queryText,
        //                new Dictionary<string, object>
        //                {
        //                    {"p0", 123}
        //                },
        //                CypherResultMode.Projection,
        //                CypherResultFormat.Rest);
        //
        //             var cypherApiQuery = new CypherApiQuery(cypherQuery);
        //
        //             using (var testHarness = new RestTestHarness
        //                 {
        //                    {
        //                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
        //                        MockResponse.Json(HttpStatusCode.OK, @"{
        //                                'data' : [ [ {
        //                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
        //                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
        //                                'property' : 'http://foo/db/data/node/0/properties/{key}',
        //                                'self' : 'http://foo/db/data/node/0',
        //                                'properties' : 'http://foo/db/data/node/0/properties',
        //                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
        //                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
        //                                'extensions' : {
        //                                },
        //                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
        //                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
        //                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
        //                                }, 'HOSTS', 'foo', 44321 ], [ {
        //                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
        //                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
        //                                'property' : 'http://foo/db/data/node/0/properties/{key}',
        //                                'self' : 'http://foo/db/data/node/2',
        //                                'properties' : 'http://foo/db/data/node/0/properties',
        //                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
        //                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
        //                                'extensions' : {
        //                                },
        //                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
        //                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
        //                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
        //                                }, 'LIKES', 'bar', 44311 ], [ {
        //                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
        //                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
        //                                'property' : 'http://foo/db/data/node/0/properties/{key}',
        //                                'self' : 'http://foo/db/data/node/12',
        //                                'properties' : 'http://foo/db/data/node/0/properties',
        //                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
        //                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
        //                                'extensions' : {
        //                                },
        //                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
        //                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
        //                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
        //                                }, 'HOSTS', 'baz', 42586 ] ],
        //                                'columns' : [ 'Fooness', 'RelationshipType', 'Name', 'UniqueId' ]
        //                            }")
        //                    }
        //                })
        //             {
        //                 var graphClient = testHarness.CreateAndConnectGraphClient();
        //
        //                 //Act
        //                 var results = graphClient.ExecuteGetCypherResults<ResultWithNodeDto>(cypherQuery);
        //
        //                 //Assert
        //                 Assert.IsAssignableFrom<IEnumerable<ResultWithNodeDto>>(results);
        //
        //                 var resultsArray = results.ToArray();
        //                 Assert.Equal(3, resultsArray.Count());
        //
        //                 var firstResult = resultsArray[0];
        //                 Assert.Equal(0, firstResult.Fooness.Reference.Id);
        //                 Assert.Equal("bar", firstResult.Fooness.Data.Bar);
        //                 Assert.Equal("baz", firstResult.Fooness.Data.Baz);
        //                 Assert.Equal("HOSTS", firstResult.RelationshipType);
        //                 Assert.Equal("foo", firstResult.Name);
        //                 Assert.Equal(44321, firstResult.UniqueId);
        //
        //                 var secondResult = resultsArray[1];
        //                 Assert.Equal(2, secondResult.Fooness.Reference.Id);
        //                 Assert.Equal("bar", secondResult.Fooness.Data.Bar);
        //                 Assert.Equal("baz", secondResult.Fooness.Data.Baz);
        //                 Assert.Equal("LIKES", secondResult.RelationshipType);
        //                 Assert.Equal("bar", secondResult.Name);
        //                 Assert.Equal(44311, secondResult.UniqueId);
        //
        //                 var thirdResult = resultsArray[2];
        //                 Assert.Equal(12, thirdResult.Fooness.Reference.Id);
        //                 Assert.Equal("bar", thirdResult.Fooness.Data.Bar);
        //                 Assert.Equal("baz", thirdResult.Fooness.Data.Baz);
        //                 Assert.Equal("HOSTS", thirdResult.RelationshipType);
        //                 Assert.Equal("baz", thirdResult.Name);
        //                 Assert.Equal(42586, thirdResult.UniqueId);
        //             }
        //        }
        //
        //        [Fact]
        //        public void ShouldDeserializeTableStructureWithNodeDataObjects()
        //        {
        //            // Arrange
        //            const string queryText = @"
        //                START x = node({p0})
        //                MATCH x-[r]->n
        //                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
        //                LIMIT 3";
        //            var cypherQuery = new CypherQuery(
        //                queryText,
        //                new Dictionary<string, object>
        //                    {
        //                        {"p0", 123}
        //                    },
        //                CypherResultMode.Projection,
        //                CypherResultFormat.Rest);
        //
        //            var cypherApiQuery = new CypherApiQuery(cypherQuery);
        //
        //            using (var testHarness = new RestTestHarness
        //                {
        //                    {
        //                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
        //                        MockResponse.Json(HttpStatusCode.OK, @"{
        //                                'data' : [ [ {
        //                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
        //                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
        //                                'property' : 'http://foo/db/data/node/0/properties/{key}',
        //                                'self' : 'http://foo/db/data/node/0',
        //                                'properties' : 'http://foo/db/data/node/0/properties',
        //                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
        //                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
        //                                'extensions' : {
        //                                },
        //                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
        //                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
        //                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
        //                                }, 'HOSTS', 'foo', 44321 ], [ {
        //                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
        //                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
        //                                'property' : 'http://foo/db/data/node/0/properties/{key}',
        //                                'self' : 'http://foo/db/data/node/2',
        //                                'properties' : 'http://foo/db/data/node/0/properties',
        //                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
        //                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
        //                                'extensions' : {
        //                                },
        //                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
        //                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
        //                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
        //                                }, 'LIKES', 'bar', 44311 ], [ {
        //                                'outgoing_relationships' : 'http://foo/db/data/node/0/relationships/out',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'traverse' : 'http://foo/db/data/node/0/traverse/{returnType}',
        //                                'all_typed_relationships' : 'http://foo/db/data/node/0/relationships/all/{-list|&|types}',
        //                                'property' : 'http://foo/db/data/node/0/properties/{key}',
        //                                'self' : 'http://foo/db/data/node/12',
        //                                'properties' : 'http://foo/db/data/node/0/properties',
        //                                'outgoing_typed_relationships' : 'http://foo/db/data/node/0/relationships/out/{-list|&|types}',
        //                                'incoming_relationships' : 'http://foo/db/data/node/0/relationships/in',
        //                                'extensions' : {
        //                                },
        //                                'create_relationship' : 'http://foo/db/data/node/0/relationships',
        //                                'paged_traverse' : 'http://foo/db/data/node/0/paged/traverse/{returnType}{?pageSize,leaseTime}',
        //                                'all_relationships' : 'http://foo/db/data/node/0/relationships/all',
        //                                'incoming_typed_relationships' : 'http://foo/db/data/node/0/relationships/in/{-list|&|types}'
        //                                }, 'HOSTS', 'baz', 42586 ] ],
        //                                'columns' : [ 'Fooness', 'RelationshipType', 'Name', 'UniqueId' ]
        //                            }")
        //                    }
        //                })
        //            {
        //                var graphClient = testHarness.CreateAndConnectGraphClient();
        //
        //                //Act
        //                var results = graphClient.ExecuteGetCypherResults<ResultWithNodeDataObjectsDto>(cypherQuery);
        //
        //                //Assert
        //                Assert.IsAssignableFrom<IEnumerable<ResultWithNodeDataObjectsDto>>(results);
        //
        //                var resultsArray = results.ToArray();
        //                Assert.Equal(3, resultsArray.Count());
        //
        //                var firstResult = resultsArray[0];
        //                Assert.Equal("bar", firstResult.Fooness.Bar);
        //                Assert.Equal("baz", firstResult.Fooness.Baz);
        //                Assert.Equal("HOSTS", firstResult.RelationshipType);
        //                Assert.Equal("foo", firstResult.Name);
        //                Assert.Equal(44321, firstResult.UniqueId);
        //
        //                var secondResult = resultsArray[1];
        //                Assert.Equal("bar", secondResult.Fooness.Bar);
        //                Assert.Equal("baz", secondResult.Fooness.Baz);
        //                Assert.Equal("LIKES", secondResult.RelationshipType);
        //                Assert.Equal("bar", secondResult.Name);
        //                Assert.Equal(44311, secondResult.UniqueId);
        //
        //                var thirdResult = resultsArray[2];
        //                Assert.Equal("bar", thirdResult.Fooness.Bar);
        //                Assert.Equal("baz", thirdResult.Fooness.Baz);
        //                Assert.Equal("HOSTS", thirdResult.RelationshipType);
        //                Assert.Equal("baz", thirdResult.Name);
        //                Assert.Equal(42586, thirdResult.UniqueId);
        //            }
        //        }
        //
        //        [Fact]
        //        public void ShouldDeserializeTableStructureWithRelationships()
        //        {
        //            // Arrange
        //            const string queryText = @"
        //                START x = node({p0})
        //                MATCH x-[r]->n
        //                RETURN x AS Fooness, type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
        //                LIMIT 3";
        //            var cypherQuery = new CypherQuery(
        //                queryText,
        //                new Dictionary<string, object>
        //                {
        //                    {"p0", 123}
        //                },
        //                CypherResultMode.Projection,
        //                CypherResultFormat.Rest);
        //
        //            var cypherApiQuery = new CypherApiQuery(cypherQuery);
        //
        //            using (var testHarness = new RestTestHarness
        //                {
        //                    {
        //                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
        //                        MockResponse.Json(HttpStatusCode.OK, @"{
        //                                'data' : [ [ {
        //                                'start' : 'http://foo/db/data/node/0',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'property' : 'http://foo/db/data/relationship/0/properties/{key}',
        //                                'self' : 'http://foo/db/data/relationship/0',
        //                                'properties' : 'http://foo/db/data/relationship/0/properties',
        //                                'type' : 'HAS_REFERENCE_DATA',
        //                                'extensions' : {
        //                                },
        //                                'end' : 'http://foo/db/data/node/1'
        //                                }, 'HOSTS', 'foo', 44321 ], [ {
        //                                'start' : 'http://foo/db/data/node/1',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'property' : 'http://foo/db/data/relationship/1/properties/{key}',
        //                                'self' : 'http://foo/db/data/relationship/1',
        //                                'properties' : 'http://foo/db/data/relationship/1/properties',
        //                                'type' : 'HAS_REFERENCE_DATA',
        //                                'extensions' : {
        //                                },
        //                                'end' : 'http://foo/db/data/node/1'
        //                                }, 'LIKES', 'bar', 44311 ], [ {
        //                                'start' : 'http://foo/db/data/node/2',
        //                                'data' : {
        //                                    'Bar' : 'bar',
        //                                    'Baz' : 'baz'
        //                                },
        //                                'property' : 'http://foo/db/data/relationship/2/properties/{key}',
        //                                'self' : 'http://foo/db/data/relationship/2',
        //                                'properties' : 'http://foo/db/data/relationship/2/properties',
        //                                'type' : 'HAS_REFERENCE_DATA',
        //                                'extensions' : {
        //                                },
        //                                'end' : 'http://foo/db/data/node/1'
        //                                }, 'HOSTS', 'baz', 42586 ] ],
        //                                'columns' : [ 'Fooness', 'RelationshipType', 'Name', 'UniqueId' ]
        //                            }")
        //                    }
        //                })
        //            {
        //                var graphClient = testHarness.CreateAndConnectGraphClient();
        //
        //                //Act
        //                //Act
        //                var results = graphClient.ExecuteGetCypherResults<ResultWithRelationshipDto>(cypherQuery);
        //
        //                //Assert
        //                Assert.IsAssignableFrom<IEnumerable<ResultWithRelationshipDto>>(results);
        //
        //                var resultsArray = results.ToArray();
        //                Assert.Equal(3, resultsArray.Count());
        //
        //                var firstResult = resultsArray[0];
        //                Assert.Equal(0, firstResult.Fooness.Reference.Id);
        //                Assert.Equal("bar", firstResult.Fooness.Data.Bar);
        //                Assert.Equal("baz", firstResult.Fooness.Data.Baz);
        //                Assert.Equal("HOSTS", firstResult.RelationshipType);
        //                Assert.Equal("foo", firstResult.Name);
        //                Assert.Equal(44321, firstResult.UniqueId);
        //
        //                var secondResult = resultsArray[1];
        //                Assert.Equal(1, secondResult.Fooness.Reference.Id);
        //                Assert.Equal("bar", secondResult.Fooness.Data.Bar);
        //                Assert.Equal("baz", secondResult.Fooness.Data.Baz);
        //                Assert.Equal("LIKES", secondResult.RelationshipType);
        //                Assert.Equal("bar", secondResult.Name);
        //                Assert.Equal(44311, secondResult.UniqueId);
        //
        //                var thirdResult = resultsArray[2];
        //                Assert.Equal(2, thirdResult.Fooness.Reference.Id);
        //                Assert.Equal("bar", thirdResult.Fooness.Data.Bar);
        //                Assert.Equal("baz", thirdResult.Fooness.Data.Baz);
        //                Assert.Equal("HOSTS", thirdResult.RelationshipType);
        //                Assert.Equal("baz", thirdResult.Name);
        //                Assert.Equal(42586, thirdResult.UniqueId);
        //            }
        //        }
        //
        //        [Fact]
        //        public void ShouldPromoteBadQueryResponseToNiceException()
        //        {
        //            // Arrange
        //            const string queryText = @"broken query";
        //            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Rest);
        //            var cypherApiQuery = new CypherApiQuery(cypherQuery);
        //
        //            using (var testHarness = new RestTestHarness
        //                {
        //                    {
        //                        MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
        //                        MockResponse.Json(HttpStatusCode.BadRequest, @"{
        //  'message' : 'expected START or CREATE\n\'bad query\'\n ^',
        //  'exception' : 'SyntaxException',
        //  'fullname' : 'org.neo4j.cypher.SyntaxException',
        //  'stacktrace' : [ 'org.neo4j.cypher.internal.parser.v1_9.CypherParserImpl.parse(CypherParserImpl.scala:45)', 'org.neo4j.cypher.CypherParser.parse(CypherParser.scala:44)', 'org.neo4j.cypher.ExecutionEngine$$anonfun$prepare$1.apply(ExecutionEngine.scala:80)', 'org.neo4j.cypher.ExecutionEngine$$anonfun$prepare$1.apply(ExecutionEngine.scala:80)', 'org.neo4j.cypher.internal.LRUCache.getOrElseUpdate(LRUCache.scala:37)', 'org.neo4j.cypher.ExecutionEngine.prepare(ExecutionEngine.scala:80)', 'org.neo4j.cypher.ExecutionEngine.execute(ExecutionEngine.scala:72)', 'org.neo4j.cypher.ExecutionEngine.execute(ExecutionEngine.scala:76)', 'org.neo4j.cypher.javacompat.ExecutionEngine.execute(ExecutionEngine.java:79)', 'org.neo4j.server.rest.web.CypherService.cypher(CypherService.java:94)', 'java.lang.reflect.Method.invoke(Unknown Source)', 'org.neo4j.server.rest.security.SecurityFilter.doFilter(SecurityFilter.java:112)' ]
        //}")
        //                    }
        //                })
        //            {
        //                var graphClient = testHarness.CreateAndConnectGraphClient();
        //
        //                var ex = Assert.Throws<NeoException>(() => graphClient.ExecuteGetCypherResults<ResultWithRelationshipDto>(cypherQuery));
        //                Assert.Equal("SyntaxException: expected START or CREATE\n'bad query'\n ^", ex.Message);
        //                Assert.Equal("expected START or CREATE\n'bad query'\n ^", ex.NeoMessage);
        //                Assert.Equal("SyntaxException", ex.NeoExceptionName);
        //                Assert.Equal("org.neo4j.cypher.SyntaxException", ex.NeoFullName);
        //
        //                var expectedStack = new[]
        //                {
        //                    "org.neo4j.cypher.internal.parser.v1_9.CypherParserImpl.parse(CypherParserImpl.scala:45)",
        //                    "org.neo4j.cypher.CypherParser.parse(CypherParser.scala:44)",
        //                    "org.neo4j.cypher.ExecutionEngine$$anonfun$prepare$1.apply(ExecutionEngine.scala:80)",
        //                    "org.neo4j.cypher.ExecutionEngine$$anonfun$prepare$1.apply(ExecutionEngine.scala:80)",
        //                    "org.neo4j.cypher.internal.LRUCache.getOrElseUpdate(LRUCache.scala:37)",
        //                    "org.neo4j.cypher.ExecutionEngine.prepare(ExecutionEngine.scala:80)",
        //                    "org.neo4j.cypher.ExecutionEngine.execute(ExecutionEngine.scala:72)",
        //                    "org.neo4j.cypher.ExecutionEngine.execute(ExecutionEngine.scala:76)",
        //                    "org.neo4j.cypher.javacompat.ExecutionEngine.execute(ExecutionEngine.java:79)",
        //                    "org.neo4j.server.rest.web.CypherService.cypher(CypherService.java:94)",
        //                    "java.lang.reflect.Method.invoke(Unknown Source)",
        //                    "org.neo4j.server.rest.security.SecurityFilter.doFilter(SecurityFilter.java:112)"
        //                };
        //                Assert.Equal(expectedStack, ex.NeoStackTrace);
        //            }
        //        }
        //
        //        [Fact]
        //        public void SendsCommandWithCorrectTimeout()
        //        {
        //            const int expectedMaxExecutionTime = 100;
        //
        //            const string queryText = @"START d=node({p0}), e=node({p1})
        //                                        MATCH p = allShortestPaths( d-[*..15]-e )
        //                                        RETURN p";
        //
        //            var parameters = new Dictionary<string, object>
        //                {
        //                    {"p0", 215},
        //                    {"p1", 219}
        //                };
        //
        //
        //            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set,CypherResultFormat.Transactional ,maxExecutionTime: expectedMaxExecutionTime);
        //            var cypherApiQuery = new CypherApiQuery(cypherQuery);
        //
        //            using (var testHarness = new RestTestHarness
        //            {
        //                {
        //                    MockRequest.Get(""),
        //                    MockResponse.NeoRoot()
        //                },
        //                {
        //                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
        //                    MockResponse.Json(HttpStatusCode.OK,
        //                    @"{
        //                              'data' : [ [ {
        //                                'start' : 'http://foo/db/data/node/215',
        //                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/0', 'http://foo/db/data/node/219' ],
        //                                'length' : 2,
        //                                'relationships' : [ 'http://foo/db/data/relationship/247', 'http://foo/db/data/relationship/257' ],
        //                                'end' : 'http://foo/db/data/node/219'
        //                              } ], [ {
        //                                'start' : 'http://foo/db/data/node/215',
        //                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/1', 'http://foo/db/data/node/219' ],
        //                                'length' : 2,
        //                                'relationships' : [ 'http://foo/db/data/relationship/248', 'http://foo/db/data/relationship/258' ],
        //                                'end' : 'http://foo/db/data/node/219'
        //                              } ] ],
        //                              'columns' : [ 'p' ]
        //                            }")
        //                }
        //            })
        //            {
        //                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);
        //                var graphClient = new GraphClient(new Uri(testHarness.BaseUri), httpClient);
        //                graphClient.Connect();
        //
        //                httpClient.ClearReceivedCalls();
        //                ((IRawGraphClient)graphClient).ExecuteGetCypherResults<object>(cypherQuery);
        //
        //                var call = httpClient.ReceivedCalls().Single();
        //                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
        //                var maxExecutionTimeHeader = requestMessage.Headers.Single(h => h.Key == "max-execution-time");
        //                Assert.Equal(expectedMaxExecutionTime.ToString(CultureInfo.InvariantCulture), maxExecutionTimeHeader.Value.Single());
        //            }
        //        }
        //
        //        [Fact]
        //        public void DoesntSendMaxExecutionTime_WhenNotAddedToQuery()
        //        {
        //            const string queryText = @"START d=node({p0}), e=node({p1})
        //                                        MATCH p = allShortestPaths( d-[*..15]-e )
        //                                        RETURN p";
        //
        //            var parameters = new Dictionary<string, object>
        //                {
        //                    {"p0", 215},
        //                    {"p1", 219}
        //                };
        //
        //            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set);
        //            var cypherApiQuery = new CypherApiQuery(cypherQuery);
        //
        //            using (var testHarness = new RestTestHarness
        //            {
        //                {
        //                    MockRequest.Get(""),
        //                    MockResponse.NeoRoot()
        //                },
        //                {
        //                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
        //                    MockResponse.Json(HttpStatusCode.OK,
        //                    @"{
        //                              'data' : [ [ {
        //                                'start' : 'http://foo/db/data/node/215',
        //                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/0', 'http://foo/db/data/node/219' ],
        //                                'length' : 2,
        //                                'relationships' : [ 'http://foo/db/data/relationship/247', 'http://foo/db/data/relationship/257' ],
        //                                'end' : 'http://foo/db/data/node/219'
        //                              } ], [ {
        //                                'start' : 'http://foo/db/data/node/215',
        //                                'nodes' : [ 'http://foo/db/data/node/215', 'http://foo/db/data/node/1', 'http://foo/db/data/node/219' ],
        //                                'length' : 2,
        //                                'relationships' : [ 'http://foo/db/data/relationship/248', 'http://foo/db/data/relationship/258' ],
        //                                'end' : 'http://foo/db/data/node/219'
        //                              } ] ],
        //                              'columns' : [ 'p' ]
        //                            }")
        //                }
        //            })
        //            {
        //                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);
        //                var graphClient = new GraphClient(new Uri(testHarness.BaseUri), httpClient);
        //                graphClient.Connect();
        //
        //                httpClient.ClearReceivedCalls();
        //                ((IRawGraphClient)graphClient).ExecuteGetCypherResults<object>(cypherQuery);
        //
        //                var call = httpClient.ReceivedCalls().Single();
        //                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
        //                Assert.False(requestMessage.Headers.Any(h => h.Key == "max-execution-time"));
        //            }
        //        }
    }
}