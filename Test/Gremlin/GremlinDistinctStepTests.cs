using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class GremlinDistinctStepTests
    {
        [Test]
        public void GremlinDistinctAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().GremlinDistinct();
            Assert.IsInstanceOf<IGremlinNodeQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outV.dedup()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void GremlinDistinctAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().GremlinDistinct();
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("g.v(p0).outE.dedup()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void GremlinDistinctAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().GremlinDistinct();
            Assert.IsInstanceOf<IGremlinRelationshipQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outE.dedup()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }
    }
}