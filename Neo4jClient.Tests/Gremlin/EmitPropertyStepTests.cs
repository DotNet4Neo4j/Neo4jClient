using Neo4jClient.Gremlin;
using Xunit;

namespace Neo4jClient.Tests.Gremlin
{
    
    public class EmitPropertyStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void EmitPropertyShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().EmitProperty("foo");
            Assert.IsAssignableFrom<IGremlinNodeQuery<object>>(query);
            Assert.Equal("g.v(p0).outV.foo", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void EmitPropertyShouldAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().EmitProperty("foo");
            Assert.IsAssignableFrom<IGremlinRelationshipQuery>(query);
            Assert.Equal("g.v(p0).outE.foo", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void EmitPropertyShouldAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().EmitProperty("foo");
            Assert.IsAssignableFrom<IGremlinRelationshipQuery<object>>(query);
            Assert.Equal("g.v(p0).outE.foo", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }
    }
}