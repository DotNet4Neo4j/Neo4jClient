using System;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
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

    public class CypherFluentQueryRuntimeTests : IClassFixture<CultureInfoSetupFixture>
    {
        //Tests:
        // 2 - must be first in the list 

        [Fact]
        public void MustBeFirst()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher35);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = new CypherFluentQuery(client)
                    .Match("(n)")
                    .Runtime("Free")
                    .Query;
            });
        }

        [Fact]
        public void TestRuntimeWithFreeTextConstruction()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher35);

            var query = new CypherFluentQuery(client)
                .Runtime("Free")
                .Query;

            Assert.Equal("CYPHER runtime=Free", query.QueryText);
        }

        [Theory]
        [InlineData(CypherRuntime.Slotted, "slotted")]
        [InlineData(CypherRuntime.Pipelined, "pipelined")]
        [InlineData(CypherRuntime.Parallel, "parallel")]
        public void TestPlannerWithCypherPlannerVariant(CypherRuntime input, string expected)
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher35);

            var query = new CypherFluentQuery(client)
                .Runtime(input)
                .Query;

            Assert.Equal($"CYPHER runtime={expected}", query.QueryText);
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenNeo4jVersionIsLessThan35()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher30);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = new CypherFluentQuery(client)
                    .Runtime("Free")
                    .Query;
            });
        }
    }
}
