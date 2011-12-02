using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class FairMergeStepTests
    {
        [Test]
        public void FairMergeShouldAppendStepToNodeQuery()
        {
            var query = new NodeReference(123).OutV<object>().FairMerge();
            Assert.IsInstanceOf<IGremlinNodeQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outV.fairMerge", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void FairMergeShouldAppendStepToRelationshipQuery()
        {
            var query = new NodeReference(123).OutE().FairMerge();
            Assert.IsInstanceOf<IGremlinRelationshipQuery>(query);
            Assert.AreEqual("g.v(p0).outE.fairMerge", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void FairMergeShouldAppendStepToTypedRelationshipQuery()
        {
            var query = new NodeReference(123).OutE<object>().FairMerge();
            Assert.IsInstanceOf<IGremlinRelationshipQuery<object>>(query);
            Assert.AreEqual("g.v(p0).outE.fairMerge", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }
    }
}