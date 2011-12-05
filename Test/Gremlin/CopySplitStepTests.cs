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
        public void CopySplitVShouldMoveInlineBlockVariablesToTheOuterScopeInFinallyQueryUsingAggregateV()
        {
            var query = new NodeReference(123).CopySplit(new IdentityPipe().OutE<object>("foo").AggregateV<object>("xyz"), new IdentityPipe().OutE<object>("bar")).OutE("baz");
            Assert.AreEqual("xyz = [];g.v(p0)._.copySplit(_().outE[[label:p1]].aggregate(xyz), _().outE[[label:p2]]).outE[[label:p3]]", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("bar", query.QueryParameters["p2"]);
            Assert.AreEqual("baz", query.QueryParameters["p3"]);
        }

        [Test]
        public void CopySplitVShouldMoveInlineBlockVariablesToTheOuterScopeInFinallyQueryUsingStoreV()
        {
            var query = new NodeReference(123).CopySplit(new IdentityPipe().OutE<object>("foo").StoreV<object>("xyz"), new IdentityPipe().OutE<object>("bar")).OutE("baz");
            Assert.AreEqual("xyz = [];g.v(p0)._.copySplit(_().outE[[label:p1]].sideEffect{xyz.add(it)}, _().outE[[label:p2]]).outE[[label:p3]]", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("bar", query.QueryParameters["p2"]);
            Assert.AreEqual("baz", query.QueryParameters["p3"]);
        }

        [Test]
        public void CopySplitVShouldMoveInlineBlockVariablesToTheOuterScopeInFinallyQueryUsingStoreVAndFilters()
        {
            var query = new NodeReference(123).CopySplit(new IdentityPipe().Out<Test>("foo", t=> t.Flag == true).StoreV<object>("xyz"), new IdentityPipe().OutE<Test>("bar")).Out<Test>("baz", t=> t.Flag == true );
            Assert.AreEqual("xyz = [];g.v(p0)._.copySplit(_().outE[[label:p1]].inV.filter{ it[p2] == p3 }.sideEffect{xyz.add(it)}, _().outE[[label:p4]]).outE[[label:p5]].inV.filter{ it[p6] == p7 }", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("Flag", query.QueryParameters["p2"]);
            Assert.AreEqual(true, query.QueryParameters["p3"]);
            Assert.AreEqual("bar", query.QueryParameters["p4"]);
            Assert.AreEqual("baz", query.QueryParameters["p5"]);
            Assert.AreEqual("Flag", query.QueryParameters["p6"]);
            Assert.AreEqual(true, query.QueryParameters["p7"]);
        }

        [Test]
        public void ShouldNumberParamtersCorrectlyInNestedQueryWithMoreThan10ParametersInTotal()
        {
            var query = new NodeReference(0)
                .CopySplit(
                    new IdentityPipe()
                        .Out<Test>("REL1", a => a.Text == "text 1")
                        .In<Test>("REL2")
                        .Out<Test>("REL3")
                        .In<Test>("REL4")
                        .In<Test>("REL5", r => r.Flag == false)
                        .StoreV<Test>("ReferralWithCentres"),
                    new IdentityPipe()
                        .Out<Test>("REL6", a => a.Text == "text 2")
                );
            Assert.AreEqual(
                "ReferralWithCentres = [];g.v(p0)._.copySplit(_().outE[[label:p1]].inV.filter{ it[p2].equalsIgnoreCase(p3) }.inE[[label:p4]].outV.outE[[label:p5]].inV.inE[[label:p6]].outV.inE[[label:p7]].outV.filter{ it[p8] == p9 }.sideEffect{ReferralWithCentres.add(it)}, _().outE[[label:p10]].inV.filter{ it[p11].equalsIgnoreCase(p12) })",
                query.QueryText);
        }

        public class Test
        {
            public bool Flag { get; set; }
            public string Text { get; set; }
        }
    }
}