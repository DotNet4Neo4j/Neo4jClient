using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class GremlinDistinctStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void GremlinDistinctAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().GremlinDistinct();
            Assert.IsAssignableFrom<IGremlinNodeQuery<object>>(query);
            Assert.Equal("g.v(p0).outV.dedup()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void GremlinDistinctAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().GremlinDistinct();
            Assert.IsAssignableFrom<IGremlinRelationshipQuery>(query);
            Assert.Equal("g.v(p0).outE.dedup()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void GremlinDistinctAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().GremlinDistinct();
            Assert.IsAssignableFrom<IGremlinRelationshipQuery<object>>(query);
            Assert.Equal("g.v(p0).outE.dedup()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }
    }
}