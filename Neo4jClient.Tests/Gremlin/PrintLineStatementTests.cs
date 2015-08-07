using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class PrintLineStatementTests
    {
        [Test]
        public void PrintLineShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).IfThenElse(
               new GremlinIterator().OutV<object>().GremlinHasNext(),
               new Statement().PrintLine("\"{$it} Hello\""),
               new Statement().PrintLine("\"{$it} GoodBye\""));
            Assert.AreEqual("g.v(p0).ifThenElse{it.outV.hasNext()}{println \"{$it} Hello\"}{println \"{$it} GoodBye\"}", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }
    }
}