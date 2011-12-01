using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class EmitVariableStepTests
    {
        [Test]
        public void EmitVariableShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().EmitVariableV<object>("x");
            Assert.IsInstanceOf<IGremlinNodeQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outV.p1", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("x", query.QueryParameters["p1"]);
        }

        [Test]
        public void EmitVariableShouldAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().EmitVariableE("x");
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("g.v(p0).outE.p1", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("x", query.QueryParameters["p1"]);
        }

        [Test]
        public void EmitVariableShouldAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().EmitVariableE<object>("x");
            Assert.IsInstanceOf<IGremlinRelationshipQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outE.p1", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("x", query.QueryParameters["p1"]);
        }
    }
}