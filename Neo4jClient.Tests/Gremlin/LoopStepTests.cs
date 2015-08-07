using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class LoopStepTests
    {
        [Test]
        public void LoopVShouldAppendStep()
        {
            var query = new NodeReference(123).LoopV<object>("foo", 6);
            Assert.AreEqual("g.v(p0).loop(p1){ it.loops < p2 }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual(6, query.QueryParameters["p2"]);
        }

        [Test]
        public void LoopVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).LoopV<object>("foo", 6);
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
        }

        [Test]
        public void LoopEShouldAppendStep()
        {
            var query = new NodeReference(123).LoopE("foo", 6);
            Assert.AreEqual("g.v(p0).loop(p1){ it.loops < p2 }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual(6, query.QueryParameters["p2"]);
        }

        [Test]
        public void LoopEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).LoopE("foo", 6);
            Assert.IsInstanceOf<GremlinRelationshipEnumerable>(query);
        }

        [Test]
        public void LoopEWithTDataShouldAppendStep()
        {
            var query = new NodeReference(123).LoopE<object>("foo", 6);
            Assert.AreEqual("g.v(p0).loop(p1){ it.loops < p2 }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual(6, query.QueryParameters["p2"]);
        }

        [Test]
        public void LoopEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).LoopE<object>("foo", 6);
            Assert.IsInstanceOf<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}
