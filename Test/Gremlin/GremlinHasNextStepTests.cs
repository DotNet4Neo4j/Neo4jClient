using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class GremlinHasNextStepTests
    {
        [Test]
        public void GremlinHasNextAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().GremlinHasNext();
            Assert.IsInstanceOf<IGremlinNodeQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outV.hasNext()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void GremlinHasNextAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().GremlinHasNext();
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("g.v(p0).outE.hasNext()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void GremlinHasNextAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().GremlinHasNext();
            Assert.IsInstanceOf<IGremlinRelationshipQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outE.hasNext()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }
    }
}