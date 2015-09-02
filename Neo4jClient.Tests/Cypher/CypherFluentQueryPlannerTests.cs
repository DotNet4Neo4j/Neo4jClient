using System;
using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    /// <summary>
    ///     Tests for the UNWIND operator
    /// </summary>
    [TestFixture]
    public class CypherFluentQueryPlannerTests
    {
        [Test]
        public void TestPlannerWithFreeTextConstruction()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher22);

            var query = new CypherFluentQuery(client)
                .Planner("FreePlanner")
                .Query;

            Assert.AreEqual("PLANNER FreePlanner", query.QueryText);
        }

        [Test, Sequential]
        public void TestPlannerWithCypherPlannerVariant(
            [Values(CypherPlanner.CostGreedy,CypherPlanner.CostIdp, CypherPlanner.Rule)] CypherPlanner input, 
            [Values("COST", "IDP", "RULE")] string expected)
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher22);

            var query = new CypherFluentQuery(client)
                .Planner(input)
                .Query;

            Assert.AreEqual(string.Format("PLANNER {0}", expected), query.QueryText);
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void ThrowsInvalidOperationException_WhenNeo4jVersionIsLessThan22()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher19);

            var _ = new CypherFluentQuery(client)
                .Planner("FreePlanner")
                .Query;
        }
    }
}
