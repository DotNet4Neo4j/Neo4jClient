using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class ExhaustMergeStepTests
    {
        [Test]
        public void ExhaustMergeAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().ExhaustMerge();
            Assert.IsInstanceOf<IGremlinNodeQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outV.exhaustMerge", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void ExhaustMergeAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().ExhaustMerge();
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("g.v(p0).outE.exhaustMerge", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void ExhaustMergeAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().ExhaustMerge();
            Assert.IsInstanceOf<IGremlinRelationshipQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outE.exhaustMerge", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }
    }
}