using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class GremlinHasNextStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void GremlinHasNextAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().GremlinHasNext();
            Assert.IsAssignableFrom<IGremlinNodeQuery<object>>(query);
            Assert.Equal("g.v(p0).outV.hasNext()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void GremlinHasNextAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().GremlinHasNext();
            Assert.IsAssignableFrom<IGremlinRelationshipQuery>(query);
            Assert.Equal("g.v(p0).outE.hasNext()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void GremlinHasNextAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().GremlinHasNext();
            Assert.IsAssignableFrom<IGremlinRelationshipQuery<object>>(query);
            Assert.Equal("g.v(p0).outE.hasNext()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }
    }
}