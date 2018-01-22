using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class IteratorTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void GremlinSkipVShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutV<object>().GremlinSkip<object>(5);
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
            Assert.Equal("g.v(p0).outV.drop(p1)._()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
        }

        [Fact]
        public void GremlinSkipEShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutE().GremlinSkip(5);
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable>(query);
            Assert.Equal("g.v(p0).outE.drop(p1)._()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
        }

        [Fact]
        public void GremlinSkipEWithTDataShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutE<object>().GremlinSkip<object>(5);
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable<object>>(query);
            Assert.Equal("g.v(p0).outE.drop(p1)._()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
        }

        [Fact]
        public void GremlinTakeVShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutV<object>().GremlinTake<object>(5);
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
            Assert.Equal("g.v(p0).outV.take(p1)._()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
        }

        [Fact]
        public void GremlinTakeEShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutE().GremlinTake(5);
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable>(query);
            Assert.Equal("g.v(p0).outE.take(p1)._()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
        }

        [Fact]
        public void GremlinTakeEWithTDataShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutE<object>().GremlinTake<object>(5);
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable<object>>(query);
            Assert.Equal("g.v(p0).outE.take(p1)._()", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
        }
    }
}