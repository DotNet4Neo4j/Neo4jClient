using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class CopySplitStepTests
    {
        [Test]
        public void CopySplitVShouldAppendStep()
        {
            var query = new NodeReference(123).CopySplit(new IdentityPipe().OutV<object>(), new IdentityPipe().OutV<object>());
            Assert.AreEqual("g.v(p0)._.copySplit(_().outV, _().outV)", query.QueryText);
        }

        [Test]
        public void CopySplitVShouldAppendStepAndPreserveOuterQueryParametersWithAllInlineBlocksAsIndentityPipes()
        {
            var query = new NodeReference(123).CopySplit(new IdentityPipe().OutE<object>("foo"), new IdentityPipe().OutE<object>("bar")).OutE("baz");
            Assert.AreEqual("g.v(p0)._.copySplit(_().outE[[label:p1]], _().outE[[label:p2]]).outE[[label:p3]]", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("bar", query.QueryParameters["p2"]);
            Assert.AreEqual("baz", query.QueryParameters["p3"]);
        }

        [Test]
        public void CopySplitVShouldAppendStepAndPreserveOuterQueryParametersWithOneInlineBlocksAsNodeReference()
        {
            var node = new NodeReference(456);
            var query = new NodeReference(123).CopySplit(new IdentityPipe().OutE<object>("foo"), node.OutE<object>("bar")).OutE("baz");
            Assert.AreEqual("g.v(p0)._.copySplit(_().outE[[label:p1]], g.v(p2).outE[[label:p3]]).outE[[label:p4]]", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual(456, query.QueryParameters["p2"]);
            Assert.AreEqual("bar", query.QueryParameters["p3"]);
            Assert.AreEqual("baz", query.QueryParameters["p4"]);
        }

        [Test]
        public void CopySplitVShouldMoveInlineBlockVariablesToTheOuterScopeInFinallyQuery()
        {
            var query = new NodeReference(123).CopySplit(new IdentityPipe().OutE<object>("foo").AggregateV<object>("xyz"), new IdentityPipe().OutE<object>("bar")).OutE("baz");
            Assert.AreEqual("x=[];g.v(p0)._.copySplit(_().outE[[label:p1]].aggregate(xyz), _().outE[[label:p2]]).outE[[label:p3]]", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("bar", query.QueryParameters["p2"]);
            Assert.AreEqual("baz", query.QueryParameters["p3"]);
        }
    }
}