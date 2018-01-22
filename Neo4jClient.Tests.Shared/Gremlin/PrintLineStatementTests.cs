using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class PrintLineStatementTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void PrintLineShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).IfThenElse(
               new GremlinIterator().OutV<object>().GremlinHasNext(),
               new Statement().PrintLine("\"{$it} Hello\""),
               new Statement().PrintLine("\"{$it} GoodBye\""));
            Assert.Equal("g.v(p0).ifThenElse{it.outV.hasNext()}{println \"{$it} Hello\"}{println \"{$it} GoodBye\"}", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
        }
    }
}