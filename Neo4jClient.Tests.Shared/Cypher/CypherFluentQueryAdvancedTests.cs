using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.Cypher
{
    public class CypherFluentQueryAdvancedTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ReturnColumnAlias()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IRawGraphClient>();

            client
                .ExecuteGetCypherResults<ReturnPropertyQueryResult>(Arg.Any<CypherQuery>())
                .Returns(Enumerable.Empty<ReturnPropertyQueryResult>());

            var cypher = new CypherFluentQuery(client);
            var results = cypher
                .Start("a", (NodeReference) 1)
                .Advanced.Return<ReturnPropertyQueryResult>(new ReturnExpression
                {
                    ResultFormat = CypherResultFormat.DependsOnEnvironment,
                    ResultMode = CypherResultMode.Projection,
                    Text = "a.Age AS SomethingTotallyDifferent"
                });
            Assert.Equal("START a=node(1)\r\nRETURN a.Age AS SomethingTotallyDifferent", results.Query.DebugQueryText);
            Assert.IsAssignableFrom<IEnumerable<ReturnPropertyQueryResult>>(results.Results);
        }

        public class ReturnPropertyQueryResult
        {
            public int SomethingTotallyDifferent { get; set; }
        }
    }
}
