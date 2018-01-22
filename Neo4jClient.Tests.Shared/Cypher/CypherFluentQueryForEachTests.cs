using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryForEachTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ForEachRawText()
        {
            // http://docs.neo4j.org/chunked/milestone/query-foreach.html
            // FOREACH (n IN nodes(p) | SET n.marked = TRUE)

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ForEach("(n IN nodes(p) | SET n.marked = TRUE)")
                .Query;

            Assert.Equal("FOREACH (n IN nodes(p) | SET n.marked = TRUE)", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }
    }
}
