using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class RetainStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void RetainVShouldAppendStep()
        {
            var query = new NodeReference(123).RetainV<object>("foo");
            Assert.Equal("g.v(p0).retain(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void RetainVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).RetainV<object>("foo");
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
        }

        [Fact]
        public void RetainEShouldAppendStep()
        {
            var query = new NodeReference(123).RetainE("foo");
            Assert.Equal("g.v(p0).retain(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void RetainEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).RetainE("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable>(query);
        }

        [Fact]
        public void RetainEWithTDataShouldAppendStep()
        {
            var query = new NodeReference(123).RetainE<object>("foo");
            Assert.Equal("g.v(p0).retain(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void RetainEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).RetainE<object>("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}