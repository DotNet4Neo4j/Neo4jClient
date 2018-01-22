using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class LoopStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void LoopVShouldAppendStep()
        {
            var query = new NodeReference(123).LoopV<object>("foo", 6);
            Assert.Equal("g.v(p0).loop(p1){ it.loops < p2 }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
            Assert.Equal(6U, query.QueryParameters["p2"]);
        }

        [Fact]
        public void LoopVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).LoopV<object>("foo", 6);
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
        }

        [Fact]
        public void LoopEShouldAppendStep()
        {
            var query = new NodeReference(123).LoopE("foo", 6);
            Assert.Equal("g.v(p0).loop(p1){ it.loops < p2 }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
            Assert.Equal(6U, query.QueryParameters["p2"]);
        }

        [Fact]
        public void LoopEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).LoopE("foo", 6);
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable>(query);
        }

        [Fact]
        public void LoopEWithTDataShouldAppendStep()
        {
            var query = new NodeReference(123).LoopE<object>("foo", 6);
            Assert.Equal("g.v(p0).loop(p1){ it.loops < p2 }", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
            Assert.Equal(6U, query.QueryParameters["p2"]);
        }

        [Fact]
        public void LoopEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).LoopE<object>("foo", 6);
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}
