using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class IfThenElseTests
    {
        [Test]
        public void IfThenElseVShouldAppendSteps()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<object>().GremlinHasNext(),
                null,
                null);
            Assert.AreEqual("g.v(p0).ifThenElse{it.outV.hasNext()}{}{}", query.QueryText);
        }

        [Test]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQuery()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<object>().GremlinHasNext(),
                new IdentityPipe().OutV<object>(),
                new IdentityPipe().InV<object>());
            Assert.AreEqual("g.v(p0).ifThenElse{it.outV.hasNext()}{_().outV}{_().inV}", query.QueryText);
        }

        [Test]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQueryWithParameters()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<Test>(t => t.Flag == true).GremlinHasNext(),
                new IdentityPipe().OutV<Test>(t => t.Name == "foo"),
                new IdentityPipe().InV<Test>(t => t.Name == "bar"));
            Assert.AreEqual("g.v(p0).ifThenElse{it.outV.filter{ it[p1] == p2 }.hasNext()}{_().outV.filter{ it[p3].equalsIgnoreCase(p4) }}{_().inV.filter{ it[p5].equalsIgnoreCase(p6) }}", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("Flag", query.QueryParameters["p1"]);
            Assert.AreEqual(true, query.QueryParameters["p2"]);
            Assert.AreEqual("Name", query.QueryParameters["p3"]);
            Assert.AreEqual("foo", query.QueryParameters["p4"]);
            Assert.AreEqual("Name", query.QueryParameters["p5"]);
            Assert.AreEqual("bar", query.QueryParameters["p6"]);
        }

        public class Test
        {
            public bool Flag { get; set; }
            public string Name { get; set; }
        }
    }
}