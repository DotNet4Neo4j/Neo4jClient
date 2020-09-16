using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryQueryStatsTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SetsIncludeQueryStatsToTrueWhenUsingQueryStatsProperty()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .WithQueryStats
                .Create("(n:Foo)")
                .Query;

            Assert.Equal("CREATE (n:Foo)", query.QueryText);
            Assert.True(query.IncludeQueryStats);
        }
    }
}