using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    public class CypherFluentQueryMaxExecutionTimeTests
    {
        [Test]
        public void SetsMaxExecutionTime_WhenUsingAReturnTypeQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .MaxExecutionTime(100)
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.AreEqual(100, query.MaxExecutionTime);
        }

        [Test]
        public void SetsMaxExecutionTime_WhenUsingANonReturnTypeQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .MaxExecutionTime(100)
                .Match("n")
                .Set("n.Value = 'value'")
                .Query;

            Assert.AreEqual(100, query.MaxExecutionTime);
        }

    }
}
