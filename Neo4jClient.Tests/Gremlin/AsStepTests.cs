using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class AsStepTests
    {
        [Test]
        public void AsShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().As("foo");
            Assert.IsInstanceOf<IGremlinNodeQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outV.as(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
        }

        [Test]
        public void AsShouldAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().As("foo");
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("g.v(p0).outE.as(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
        }

        [Test]
        public void AsShouldAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().As("foo");
            Assert.IsInstanceOf<IGremlinRelationshipQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outE.as(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
        }
    }
}