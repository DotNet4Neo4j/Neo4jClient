using System.Threading.Tasks;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryDatabaseTests : IClassFixture<CultureInfoSetupFixture>
    {
        internal static MockResponse EmptyOKResponse = MockResponse.Json(200, @"{'results':[], 'errors':[] }");
        internal static string EmptyStatements = "{'statements': []}";

        [Theory]
        [InlineData("FOO")]
        [InlineData("foo")]
        [InlineData("Foo")]
        [InlineData("fOO")]
        [InlineData("FoO")]
        public async Task ShouldAlwaysBeLowercase(string database)
        {
            var queryRequest = MockRequest.PostJson($"/db/{database.ToLowerInvariant()}/tx/commit",
                "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            using (var testHarness = new RestTestHarness(false, "http://foo:7474")
            {
                {queryRequest, EmptyOKResponse}
            })
            {
                var client = await testHarness.CreateAndConnectGraphClient(RestTestHarness.Neo4jVersion.Neo40);

                // dummy query to generate request
                await client.Cypher
                    .WithDatabase(database)
                    .Match("n")
                    .Return(n => n.Count())
                    .ExecuteWithoutResultsAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task ShouldBeNeo4jIfNotSet()
        {
            var queryRequest = MockRequest.PostJson("/db/neo4j/tx/commit",
                "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            using (var testHarness = new RestTestHarness(false, "http://foo:7474")
            {
                {queryRequest, EmptyOKResponse}
            })
            {
                var client = await testHarness.CreateAndConnectGraphClient(RestTestHarness.Neo4jVersion.Neo40);

                // dummy query to generate request
                await client.Cypher
                    .Match("n")
                    .Return(n => n.Count())
                    .ExecuteWithoutResultsAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task WithDatabase_ShouldOverrideDefaultDatabase()
        {
            var queryRequest = MockRequest.PostJson("/db/neo4jclient/tx/commit",
                "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            using (var testHarness = new RestTestHarness(false, "http://foo:7474")
            {
                {queryRequest, EmptyOKResponse}
            })
            {
                var client = await testHarness.CreateAndConnectGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                client.DefaultDatabase = "neo4j";

                // dummy query to generate request
                await client.Cypher
                    .WithDatabase("neo4jclient")
                    .Match("n")
                    .Return(n => n.Count())
                    .ExecuteWithoutResultsAsync().ConfigureAwait(false);
            }
        }
    }
}