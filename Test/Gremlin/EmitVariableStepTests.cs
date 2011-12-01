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
            var query = new NodeReference(123).OutV<object>().AggregateV<object>("x").OutV<object>().EmitVariableV<object>("x");
            Assert.IsInstanceOf<IGremlinNodeQuery<object>>(query);
            Assert.AreEqual("p2 = [];g.v(p0).outV.aggregate(p1).outV.p3", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("x", query.QueryParameters["p1"]);
            Assert.AreEqual("x", query.QueryParameters["p2"]);
        }

        [Test]
        public void EmitVariableShouldAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().AggregateE("x").OutE("mylabel").EmitVariableE("x");
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("p2 = [];g.v(p0).outE.aggregate(p1).outE[[label:p3]].p4", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("x", query.QueryParameters["p1"]);
            Assert.AreEqual("x", query.QueryParameters["p2"]);
            Assert.AreEqual("mylabel", query.QueryParameters["p3"]);
            Assert.AreEqual("x", query.QueryParameters["p4"]);
        }

        [Test]
        public void EmitVariableShouldAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().AggregateE("x").OutE<object>("mylabel").EmitVariableE("x");
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("p2 = [];g.v(p0).outE.aggregate(p1).outE[[label:p3]].p4", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("x", query.QueryParameters["p1"]);
            Assert.AreEqual("x", query.QueryParameters["p2"]);
            Assert.AreEqual("mylabel", query.QueryParameters["p3"]);
            Assert.AreEqual("x", query.QueryParameters["p4"]);
        }
    }
}