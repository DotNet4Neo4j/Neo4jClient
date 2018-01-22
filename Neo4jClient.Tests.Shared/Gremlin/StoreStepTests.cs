using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class StoreStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void StoreVShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).StoreV<object>("foo");
            Assert.Equal("foo = [];g.v(p0).store(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void StoreVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).StoreV<object>("foo");
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
        }

        [Fact]
        public void StoreEShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).StoreE("foo");
            Assert.Equal("foo = [];g.v(p0).store(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void StoreEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).StoreE("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable>(query);
        }

        [Fact]
        public void StoreEWithTDataShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).StoreE<object>("foo");
            Assert.Equal("foo = [];g.v(p0).store(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void StoreEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).StoreE<object>("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}