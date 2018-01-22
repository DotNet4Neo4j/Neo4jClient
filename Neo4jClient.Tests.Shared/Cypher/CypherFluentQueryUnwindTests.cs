using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.Cypher
{
    /// <summary>
    ///     Tests for the UNWIND operator
    /// </summary>
    
    public class CypherFluentQueryUnwindTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void TestUnwindConstruction()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Unwind("collection", "column")
                .Query;

            Assert.Equal("UNWIND collection AS column", query.QueryText);
        }

        [Fact]
        public void TestUnwindAfterWithTResultVariant()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(collection => new { collection })
                .Unwind("collection", "column")
                .Query;

            Assert.Equal("WITH collection\r\nUNWIND collection AS column", query.QueryText);
        }

        [Fact]
        public void TestUnwindUsingCollection()
        {
            var collection = new[] { 1, 2, 3 };
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Unwind(collection, "alias")
                .Query;

            Assert.Equal("UNWIND {p0} AS alias", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(collection, query.QueryParameters["p0"]);
        }
    }
}
