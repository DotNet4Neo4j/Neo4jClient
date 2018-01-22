using System.Linq;
using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class UnionTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-union.html#union-combine-two-queries-and-removing-duplicates")]
        public void UnionAll()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .UnionAll()
                .Query;

            Assert.Equal("UNION ALL", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-union.html#union-union-two-queries")]
        public void Union()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Union()
                .Query;

            Assert.Equal("UNION", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
        }
    }
}
