using Neo4jClient.Gremlin;
using Xunit;

namespace Neo4jClient.Tests.Gremlin
{
    
    public class AggregateStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void AggregateVShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).AggregateV<object>("foo");
            Assert.Equal("foo = [];g.v(p0).aggregate(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void AggregateVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).AggregateV<object>("foo");
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
        }

        [Fact]
        public void AggregateEShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).AggregateE("foo");
            Assert.Equal("foo = [];g.v(p0).aggregate(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void AggregateEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).AggregateE("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable>(query);
        }

        [Fact]
        public void AggregateEWithTDataShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).AggregateE<object>("foo");
            Assert.Equal("foo = [];g.v(p0).aggregate(foo)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void AggregateEWithTDataShouldAppendStepAndDeclareVariables()
        {
            var query = new NodeReference(123).AggregateE<object>("foo").AggregateE<object>("bar");
            Assert.Equal("bar = [];foo = [];g.v(p0).aggregate(foo).aggregate(bar)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void AggregateEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).AggregateE<object>("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}