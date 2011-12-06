using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class IfThenElseTests
    {
        [Test]
        public void IfThenElseVShouldAppendSteps()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<object>().GremlinHasNext(),
                null,
                null);
            Assert.AreEqual("g.v(p0).ifThenElse{it.outV.hasNext()}{}{}", query.QueryText);
        }
    }
}