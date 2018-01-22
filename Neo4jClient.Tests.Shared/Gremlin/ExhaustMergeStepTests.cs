using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class ExhaustMergeStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ExhaustMergeAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().ExhaustMerge();
            Assert.IsAssignableFrom<IGremlinNodeQuery<object>>(query);
            Assert.Equal("g.v(p0).outV.exhaustMerge", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void ExhaustMergeAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().ExhaustMerge();
            Assert.IsAssignableFrom<IGremlinRelationshipQuery>(query);
            Assert.Equal("g.v(p0).outE.exhaustMerge", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void ExhaustMergeAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().ExhaustMerge();
            Assert.IsAssignableFrom<IGremlinRelationshipQuery<object>>(query);
            Assert.Equal("g.v(p0).outE.exhaustMerge", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }
    }
}