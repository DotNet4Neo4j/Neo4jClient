using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Xunit;

namespace Neo4jClient.Tests.BoltGraphClientTests.Cypher
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
        public async Task RelationshipShouldDeserializeInDefinedType()
        {
            // Arrange
            const string queryText = "MATCH (n:Test)-[r]->(t:Test) RETURN r AS Rel";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional, "neo4j");

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

                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var results = (await graphClient.ExecuteGetCypherResultsAsync<RelationGrouper>(cypherQuery)).ToArray();

                //Assert
                results.Length.Should().Be(1);
                var relation = results.First().Rel;
                relation.Id.Should().Be(42);
            }
        }

        [Fact]
        public async Task RelationshipShouldDeserializeInAnonymousType()
        {
            // Arrange
            const string queryText = @"MATCH (n:Test)-[r]->(t:Test) RETURN r AS Rel";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional, "neo4j");

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
                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var genericGetCypherResults = typeof(IRawGraphClient).GetMethod(nameof(graphClient.ExecuteGetCypherResultsAsync));
                var anonymousGetCypherResults = genericGetCypherResults.MakeGenericMethod(anonType);
                var genericResultsTask = anonymousGetCypherResults.Invoke(graphClient, new object[] { cypherQuery });
                await (Task) genericResultsTask;
                var genericResults = (IEnumerable) ((dynamic) genericResultsTask).Result;

                var results = genericResults.Cast<object>().ToArray();

                //Assert
                Assert.Equal(1, results.Length);
                var relation = (RelationType)anonType.GetProperty(nameof(dummy.Rel)).GetValue(results.First(), null);
                relation.Id.Should().Be(42);
            }
        }

        [Fact]
        public async Task CollectionShouldDeserializeCorrectly()
        {
            // simulate a collect()
            const string queryText = "MATCH (start:Node) RETURN collect(start) AS data";

            var queryParams = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j");

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

                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var results = (await graphClient.ExecuteGetCypherResultsAsync<IEnumerable<ObjectWithIds>>(cypherQuery)).ToArray();

                //Assert
                var deserializedObject = results.First().First();
                deserializedObject.Ids.Count.Should().Be(3);
                deserializedObject.Ids[0].Should().Be(1);
                deserializedObject.Ids[1].Should().Be(2);
                deserializedObject.Ids[2].Should().Be(3);
            }
        }

        [Fact]
        public async Task EmptyCollectionShouldDeserializeCorrectly()
        {
            const string queryText = "RETURN [] AS data";

            var queryParams = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j");

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

                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var results = (await graphClient.ExecuteGetCypherResultsAsync<IEnumerable<ObjectWithIds>>(cypherQuery)).ToArray();

                results.Should().BeEmpty();
            }
        }

        //https://github.com/readify/neo4jclient/issues/266
        [Fact]
        public async Task CollectionOfComplexTypesShouldDeserializeCorrectlyWhenInConjunctionWithAnotherComplexTypeInAContainer()
        {
            const string queryText = "MATCH (start:Node)-->(next:Node) RETURN start AS Start, collect(next) AS Next";

            var queryParams = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Projection, CypherResultFormat.Transactional, "neo4j");

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

                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var results = (await graphClient.ExecuteGetCypherResultsAsync<Container>(cypherQuery)).ToArray();

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

        //see: https://github.com/DotNet4Neo4j/Neo4jClient/issues/368
        [Fact]
        public async Task ShouldBeAbleToCastToObject_WhenUsingReturnAs()
        {
            const string queryText = "MATCH (user)-[:hasPost]->(post:Post) WHERE(user.Username = 'user1') RETURN user{.Username} AS User, post AS Post";

            var queryParams = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Projection, CypherResultFormat.Transactional, "neo4j");

            using (var testHarness = new BoltTestHarness())
            {
                const string username = "User1";
                const string postName = "Post1";

                var user = new Dictionary<string, object> { { "Username", username } };
                var postNodeMock = new Mock<INode>();
                postNodeMock.Setup(n => n.Id).Returns(1);
                postNodeMock.Setup(n => n.Properties)
                    .Returns(new Dictionary<string, object> { { "Content", postName } });

                var recordMock = new Mock<IRecord>();
                recordMock
                    .Setup(r => r["User"])
                    .Returns(user);

                recordMock
                    .Setup(r => r["Post"])
                    .Returns(postNodeMock.Object);

                recordMock
                    .Setup(r => r.Keys)
                    .Returns(new[] { "User", "Post" });

                recordMock.Setup(r => r.Values)
                    .Returns(new Dictionary<string, object> { { "User", user }, { "Post", postNodeMock.Object } });

                var testStatementResult = new TestStatementResult(new[] { "User", "Post" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var results = (await graphClient.ExecuteGetCypherResultsAsync<PostAndUser>(cypherQuery)).ToArray();
                //
                //Assert
                var deserializedObject = results.First();
                deserializedObject.User.Should().NotBeNull();
                deserializedObject.User.Username.Should().Be(username);
            }

        }
        private class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private class Post
        {
            public string Content { get; set; }
        }

        private class PostAndUser
        {
            public User User { get; set; }
            public Post Post { get; set; }
        }

        [Fact]
        public async Task CreateWithArrayParametersShouldSerializeAndDeserializeOnReturn()
        {
            // Arrange
            const string queryText = "CREATE (start:Node {obj}) RETURN start";

            var testNode = new ObjectWithIds()
            {
                Ids = new List<int>() {1, 2, 3}
            };

            var queryParams = new Dictionary<string, object>() {{"obj", testNode}};

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j");

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

                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var results = (await graphClient.ExecuteGetCypherResultsAsync<ObjectWithIds>(cypherQuery)).ToArray();

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
        public async Task ShouldDeserializeMapWithAnonymousReturnAsDictionary()
        {
            // simulates the following query
            const string queryText = "MATCH (start:Node) WITH {Node: 3, Count: 1} AS Node, start RETURN Node, start";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection,
                CypherResultFormat.Transactional, "neo4j");

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
                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var genericGetCypherResults = typeof(IRawGraphClient).GetMethod(nameof(graphClient.ExecuteGetCypherResultsAsync));
                var anonymousGetCypherResults = genericGetCypherResults.MakeGenericMethod(anonType);
                var genericResultsTask = anonymousGetCypherResults.Invoke(graphClient, new object[] { cypherQuery });
                await (Task) genericResultsTask;
                var genericResults = (IEnumerable) ((dynamic) genericResultsTask).Result;;

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
        public async Task ShouldDeserializeMapWithAnonymousReturn()
        {
            // simulates the following query
            const string queryText = "MATCH (start:Node) WITH {Node: start, Count: 1} AS Node, start RETURN Node, start";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection,
                CypherResultFormat.Transactional, "neo4j");

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
                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var genericGetCypherResults = typeof(IRawGraphClient).GetMethod(nameof(graphClient.ExecuteGetCypherResultsAsync));
                var anonymousGetCypherResults = genericGetCypherResults.MakeGenericMethod(anonType);
                var genericResultsTask = anonymousGetCypherResults.Invoke(graphClient, new object[] { cypherQuery });
                await (Task) genericResultsTask;
                var genericResults = (IEnumerable) ((dynamic) genericResultsTask).Result;

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
        public async Task ShouldDeserializeCollectionsWithAnonymousReturn()
        {
            // Arrange
            const string queryText = @"MATCH (start:Node) RETURN [start.Id, start.Id] AS Ids";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional, "neo4j");

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
                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var genericGetCypherResults = typeof(IRawGraphClient).GetMethod(nameof(graphClient.ExecuteGetCypherResultsAsync));
                var anonymousGetCypherResults = genericGetCypherResults.MakeGenericMethod(anonType);
                var genericResultsTask = anonymousGetCypherResults.Invoke(graphClient, new object[] { cypherQuery });
                await (Task) genericResultsTask;
                var genericResults = (IEnumerable) ((dynamic) genericResultsTask).Result;

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
        public async Task ShouldDeserializePathsResultAsSetBased()
        {
            // Arrange
            const string queryText = @"MATCH (start:Node {Id:$p0}),(end:Node {Id: $p1}), p = shortestPath((start)-[*..5]->(end)) RETURN p";

            var parameters = new Dictionary<string, object>
            {
                {"p0", 215},
                {"p1", 219}
            };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set, CypherResultFormat.Rest, "neo4j");
            
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

                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                var results = (await graphClient.ExecuteGetCypherResultsAsync<PathsResultBolt>(cypherQuery)).ToArray();

                //Assert
                Assert.IsAssignableFrom<IEnumerable<PathsResultBolt>>(results);
                Assert.Equal(1, results.First().Length);
                results.First().Nodes.Count().Should().Be(2);
                results.First().Relationships.Count().Should().Be(1);
                results.First().End.Id.Should().Be(1);
                results.First().Start.Id.Should().Be(2);
            }
        }
    }
}