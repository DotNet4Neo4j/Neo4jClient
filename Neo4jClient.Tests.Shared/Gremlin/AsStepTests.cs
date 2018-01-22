using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class AsStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void AsShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().As("foo");
            Assert.IsAssignableFrom<IGremlinNodeQuery<object>>(query);
            Assert.Equal("g.v(p0).outV.as(p1)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
        }

        [Fact]
        public void AsShouldAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().As("foo");
            Assert.IsAssignableFrom<IGremlinRelationshipQuery>(query);
            Assert.Equal("g.v(p0).outE.as(p1)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
        }

        [Fact]
        public void AsShouldAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().As("foo");
            Assert.IsAssignableFrom<IGremlinRelationshipQuery<object>>(query);
            Assert.Equal("g.v(p0).outE.as(p1)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
        }
    }
}