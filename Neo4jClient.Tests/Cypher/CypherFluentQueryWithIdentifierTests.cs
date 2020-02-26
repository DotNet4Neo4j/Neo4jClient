using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Neo4jClient.Cypher;
using Neo4jClient.Tests.BoltGraphClientTests;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class WithIdentifierMethod : IClassFixture<CultureInfoSetupFixture>
    {
        private class ObjectWithIds
        {
            public List<int> Ids { get; set; }
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void DoesNothing_WhenIdentifierIsNullOrWhitespace(string identifier)
        {
            var mockGc = new Mock<IRawGraphClient>();

            var cfq = new CypherFluentQuery(mockGc.Object);
            cfq.WithIdentifier(identifier);
            var query = cfq.Query;
            query.Identifier.Should().BeNull();
        }

        [Fact]
        public void SetsTheIdentifer_WhenValid()
        {
            const string identifier = "MyQuery";
            var mockGc = new Mock<IRawGraphClient>();

            var cfq = new CypherFluentQuery(mockGc.Object);
            cfq.WithIdentifier(identifier);
            var query = cfq.Query;
            query.Identifier.Should().Be(identifier);
        }

        [Fact]
        public async Task ArgsContainIdentifier()
        {
            const string identifier = "identifier";

            const string queryText = "RETURN [] AS data";

            var queryParams = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j") {Identifier = identifier};

            using (var testHarness = new BoltTestHarness())
            {
                var recordMock = new Mock<IRecord>();
                recordMock.Setup(r => r["data"]).Returns(new List<INode>());
                recordMock.Setup(r => r.Keys).Returns(new[] { "data" });

                var testStatementResult = new TestStatementResult(new[] { "data" }, recordMock.Object);
                testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                graphClient.OperationCompleted += (s, e) => { e.Identifier.Should().Be(identifier); };

                await graphClient.ExecuteGetCypherResultsAsync<IEnumerable<ObjectWithIds>>(cypherQuery);
            }
        }

    }
}