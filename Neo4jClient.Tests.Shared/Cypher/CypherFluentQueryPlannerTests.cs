using System;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.Cypher
{
    /// <summary>
    ///     Tests for the UNWIND operator
    /// </summary>
    
    public class CypherFluentQueryPlannerTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void TestPlannerWithFreeTextConstruction()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher22);

            var query = new CypherFluentQuery(client)
                .Planner("FreePlanner")
                .Query;

            Assert.Equal("PLANNER FreePlanner", query.QueryText);
        }

        [Theory]
        [InlineData(CypherPlanner.CostGreedy, "COST")]
        [InlineData(CypherPlanner.CostIdp, "IDP")]
        [InlineData(CypherPlanner.Rule, "RULE")]
        public void TestPlannerWithCypherPlannerVariant(CypherPlanner input, string expected)
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher22);

            var query = new CypherFluentQuery(client)
                .Planner(input)
                .Query;

            Assert.Equal(string.Format("PLANNER {0}", expected), query.QueryText);
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenNeo4jVersionIsLessThan22()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher19);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = new CypherFluentQuery(client)
                    .Planner("FreePlanner")
                    .Query;
            });
        }
    }
}
