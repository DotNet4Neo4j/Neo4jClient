using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryAdvancedTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task ReturnColumnAlias()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IRawGraphClient>();

            client
                .ExecuteGetCypherResultsAsync<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Match("a")
                .Advanced.Return<ReturnPropertyQueryResult>(new ReturnExpression
                {
                    ResultFormat = CypherResultFormat.DependsOnEnvironment,
                    ResultMode = CypherResultMode.Projection,
                    Text = "a.Age AS SomethingTotallyDifferent"
                });
            Assert.Equal($"MATCH a{Environment.NewLine}RETURN a.Age AS SomethingTotallyDifferent", results.Query.DebugQueryText);
            Assert.IsAssignableFrom<IEnumerable<ReturnPropertyQueryResult>>(await results.ResultsAsync);
        }

        public class ReturnPropertyQueryResult
        {
            public int SomethingTotallyDifferent { get; set; }
        }
    }
}
