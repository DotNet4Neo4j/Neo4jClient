using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class EmitPropertyStepTests
    {
        [Test]
        public void EmitPropertyShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().EmitProperty("foo");
            Assert.IsInstanceOf<IGremlinNodeQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outV.foo", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void EmitPropertyShouldAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().EmitProperty("foo");
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("g.v(p0).outE.foo", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void EmitPropertyShouldAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().EmitProperty("foo");
            Assert.IsInstanceOf<IGremlinRelationshipQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outE.foo", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }
    }
}