using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class FairMergeStepTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void FairMergeShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().FairMerge();
            Assert.IsAssignableFrom<IGremlinNodeQuery<object>>(query);
            Assert.Equal("g.v(p0).outV.fairMerge", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void FairMergeShouldAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().FairMerge();
            Assert.IsAssignableFrom<IGremlinRelationshipQuery>(query);
            Assert.Equal("g.v(p0).outE.fairMerge", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void FairMergeShouldAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().FairMerge();
            Assert.IsAssignableFrom<IGremlinRelationshipQuery<object>>(query);
            Assert.Equal("g.v(p0).outE.fairMerge", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }
    }
}