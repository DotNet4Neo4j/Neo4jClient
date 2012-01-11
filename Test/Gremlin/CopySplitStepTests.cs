using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class CopySplitStepTests
    {
        [Test]
        public void CopySplitVShouldAppendStepForRelationships()
        {
            var query = new NodeReference(123).CopySplitE(new IdentityPipe().OutE<object>(), new IdentityPipe().OutE<object>());
            Assert.AreEqual("g.v(p0)._.copySplit(_().outE, _().outE)", query.QueryText);
        }

        [Test]
        public void CopySplitVShouldAppendStepForNodes()
        {
            var query = new NodeReference(123).CopySplitV<object>(new IdentityPipe().OutV<object>(), new IdentityPipe().OutV<object>());
            Assert.AreEqual("g.v(p0)._.copySplit(_().outV, _().outV)", query.QueryText);
        }

        [Test]
        public void CopySplitEShouldAppendStepAndPreserveOuterQueryParametersWithAllInlineBlocksAsIndentityPipes()
        {
            var query = new NodeReference(123).CopySplitE(new IdentityPipe().Out<object>("foo"), new IdentityPipe().Out<object>("bar")).Out<object>("baz");
            Assert.AreEqual("g.v(p0)._.copySplit(_().out(p1), _().out(p2)).out(p3)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("bar", query.QueryParameters["p2"]);
            Assert.AreEqual("baz", query.QueryParameters["p3"]);
        }

        [Test]
        public void CopySplitVShouldAppendStepAndPreserveOuterQueryParametersWithOneInlineBlocksAsNodeReference()
        {
            var node = new NodeReference(456);
            var query = new NodeReference(123).CopySplitE(new IdentityPipe().Out<object>("foo"), node.Out<object>("bar")).Out<object>("baz");
            Assert.AreEqual("g.v(p0)._.copySplit(_().out(p1), g.v(p2).out(p3)).out(p4)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual(456, query.QueryParameters["p2"]);
            Assert.AreEqual("bar", query.QueryParameters["p3"]);
            Assert.AreEqual("baz", query.QueryParameters["p4"]);
        }

        [Test]
        public void CopySplitVShouldMoveInlineBlockVariablesToTheOuterScopeInFinallyQueryUsingAggregateV()
        {
            var query = new NodeReference(123).CopySplitE(new IdentityPipe().Out<object>("foo").AggregateV<object>("xyz"), new IdentityPipe().Out<object>("bar")).Out<object>("baz");
            Assert.AreEqual("xyz = [];g.v(p0)._.copySplit(_().out(p1).aggregate(xyz), _().out(p2)).out(p3)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("bar", query.QueryParameters["p2"]);
            Assert.AreEqual("baz", query.QueryParameters["p3"]);
        }

        [Test]
        public void CopySplitVShouldMoveInlineBlockVariablesToTheOuterScopeInFinallyQueryUsingStoreV()
        {
            var query = new NodeReference(123).CopySplitE(new IdentityPipe().Out<object>("foo").StoreV<object>("xyz"), new IdentityPipe().Out<object>("bar")).Out<object>("baz");
            Assert.AreEqual("xyz = [];g.v(p0)._.copySplit(_().out(p1).sideEffect{xyz.add(it)}, _().out(p2)).out(p3)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("bar", query.QueryParameters["p2"]);
            Assert.AreEqual("baz", query.QueryParameters["p3"]);
        }

        [Test]
        public void CopySplitVShouldMoveInlineBlockVariablesToTheOuterScopeInFinallyQueryUsingStoreVAndFilters()
        {
            var query = new NodeReference(123).CopySplitE(new IdentityPipe().Out<Test>("foo", t=> t.Flag == true).StoreV<object>("xyz"), new IdentityPipe().Out<Test>("bar")).Out<Test>("baz", t=> t.Flag == true );
            Assert.AreEqual("xyz = [];g.v(p0)._.copySplit(_().out(p1).filter{ it[p2] == p3 }.sideEffect{xyz.add(it)}, _().out(p4)).out(p5).filter{ it[p6] == p7 }", query.QueryText);
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
        public void CopySplitVShouldMoveInlineBlockVariablesToTheOuterScopeInFinallyQueryUsingStoreVAndFiltersMultipleVariables()
        {
            var query = new NodeReference(123).CopySplitE(new IdentityPipe().Out<Test>("foo", t => t.Flag == true).StoreV<object>("xyz"), new IdentityPipe().Out<Test>("bar")).Out<Test>("baz", t => t.Flag == true).AggregateE("sad");
            Assert.AreEqual("sad = [];xyz = [];g.v(p0)._.copySplit(_().out(p1).filter{ it[p2] == p3 }.sideEffect{xyz.add(it)}, _().out(p4)).out(p5).filter{ it[p6] == p7 }.aggregate(sad)", query.QueryText);
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
                .CopySplitE(
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
                "ReferralWithCentres = [];g.v(p0)._.copySplit(_().out(p1).filter{ it[p2].equalsIgnoreCase(p3) }.in(p4).out(p5).in(p6).in(p7).filter{ it[p8] == p9 }.sideEffect{ReferralWithCentres.add(it)}, _().out(p10).filter{ it[p11].equalsIgnoreCase(p12) })",
                query.QueryText);
        }

        public class Test
        {
            public bool Flag { get; set; }
            public string Text { get; set; }
        }
    }
}