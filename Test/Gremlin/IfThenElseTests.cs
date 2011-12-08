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
                new GremlinIterator().OutV<object>(),
                new GremlinIterator().InV<object>());
            Assert.AreEqual("g.v(p0).ifThenElse{it.outV.hasNext()}{it.outV}{it.inV}", query.QueryText);
        }

        [Test]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQueryWithParameters()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<Test>(t => t.Flag == true).GremlinHasNext(),
                new GremlinIterator().OutV<Test>(t => t.Name == "foo"),
                new GremlinIterator().InV<Test>(t => t.Name == "bar"));
            Assert.AreEqual("g.v(p0).ifThenElse{it.outV.filter{ it[p1] == p2 }.hasNext()}{it.outV.filter{ it[p3].equalsIgnoreCase(p4) }}{it.inV.filter{ it[p5].equalsIgnoreCase(p6) }}", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("Flag", query.QueryParameters["p1"]);
            Assert.AreEqual(true, query.QueryParameters["p2"]);
            Assert.AreEqual("Name", query.QueryParameters["p3"]);
            Assert.AreEqual("foo", query.QueryParameters["p4"]);
            Assert.AreEqual("Name", query.QueryParameters["p5"]);
            Assert.AreEqual("bar", query.QueryParameters["p6"]);
        }

        [Test]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQueryWithParametersAndDeclarations()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<Test>(t => t.Flag == true).GremlinHasNext(),
                new GremlinIterator().AggregateV<object>("x").OutV<Test>(t => t.Name == "foo"),
                new GremlinIterator().InV<Test>(t => t.Name == "bar"));
            Assert.AreEqual("x = [];g.v(p0).ifThenElse{it.outV.filter{ it[p1] == p2 }.hasNext()}{it.aggregate(x).outV.filter{ it[p3].equalsIgnoreCase(p4) }}{it.inV.filter{ it[p5].equalsIgnoreCase(p6) }}", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("Flag", query.QueryParameters["p1"]);
            Assert.AreEqual(true, query.QueryParameters["p2"]);
            Assert.AreEqual("Name", query.QueryParameters["p3"]);
            Assert.AreEqual("foo", query.QueryParameters["p4"]);
            Assert.AreEqual("Name", query.QueryParameters["p5"]);
            Assert.AreEqual("bar", query.QueryParameters["p6"]);
        }

        [Test]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQueryWithParametersAndMultipleDeclarations()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<Test>(t => t.Flag == true).GremlinHasNext(),
                new GremlinIterator().AggregateV<object>("x").OutV<Test>(t => t.Name == "foo"),
                new GremlinIterator().AggregateV<object>("y").InV<Test>(t => t.Name == "bar"));
            Assert.AreEqual("y = [];x = [];g.v(p0).ifThenElse{it.outV.filter{ it[p1] == p2 }.hasNext()}{it.aggregate(x).outV.filter{ it[p3].equalsIgnoreCase(p4) }}{it.aggregate(y).inV.filter{ it[p5].equalsIgnoreCase(p6) }}", query.QueryText);
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