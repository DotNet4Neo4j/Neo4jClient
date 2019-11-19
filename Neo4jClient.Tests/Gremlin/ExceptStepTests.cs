using Neo4jClient.Gremlin;
using Xunit;

namespace Neo4jClient.Tests.Gremlin
{
    
    public class ExceptStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ExceptVShouldAppendStep()
        {
            var query = new NodeReference(123).ExceptV<object>("foo");
            Assert.Equal("g.v(p0).except(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void ExceptVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).ExceptV<object>("foo");
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
        }

        [Fact]
        public void ExceptEShouldAppendStep()
        {
            var query = new NodeReference(123).ExceptE("foo");
            Assert.Equal("g.v(p0).except(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void ExceptEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).ExceptE("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable>(query);
        }

        [Fact]
        public void ExceptEWithTDataShouldAppendStep()
        {
            var query = new NodeReference(123).ExceptE<object>("foo");
            Assert.Equal("g.v(p0).except(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void ExceptEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).ExceptE<object>("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}