using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class IfThenElseTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void IfThenElseVShouldAppendSteps()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<object>().GremlinHasNext(),
                null,
                null);
            Assert.Equal("g.v(p0).ifThenElse{it.outV.hasNext()}{}{}", query.QueryText);
        }

        [Fact]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQuery()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<object>().GremlinHasNext(),
                new GremlinIterator().OutV<object>(),
                new GremlinIterator().InV<object>());
            Assert.Equal("g.v(p0).ifThenElse{it.outV.hasNext()}{it.outV}{it.inV}", query.QueryText);
        }

        [Fact]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQueryWithParameters()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<Test>(t => t.Flag == true).GremlinHasNext(),
                new GremlinIterator().OutV<Test>(t => t.Name == "foo"),
                new GremlinIterator().InV<Test>(t => t.Name == "bar"));
            Assert.Equal("g.v(p0).ifThenElse{it.outV.filter{ it[p1] == p2 }.hasNext()}{it.outV.filter{ it[p3].equalsIgnoreCase(p4) }}{it.inV.filter{ it[p5].equalsIgnoreCase(p6) }}", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("Flag", query.QueryParameters["p1"]);
            Assert.Equal(true, query.QueryParameters["p2"]);
            Assert.Equal("Name", query.QueryParameters["p3"]);
            Assert.Equal("foo", query.QueryParameters["p4"]);
            Assert.Equal("Name", query.QueryParameters["p5"]);
            Assert.Equal("bar", query.QueryParameters["p6"]);
        }

        [Fact]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQueryWithParametersAndDeclarations()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<Test>(t => t.Flag == true).GremlinHasNext(),
                new GremlinIterator().AggregateV<object>("x").OutV<Test>(t => t.Name == "foo"),
                new GremlinIterator().InV<Test>(t => t.Name == "bar"));
            Assert.Equal("x = [];g.v(p0).ifThenElse{it.outV.filter{ it[p1] == p2 }.hasNext()}{it.aggregate(x).outV.filter{ it[p3].equalsIgnoreCase(p4) }}{it.inV.filter{ it[p5].equalsIgnoreCase(p6) }}", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("Flag", query.QueryParameters["p1"]);
            Assert.Equal(true, query.QueryParameters["p2"]);
            Assert.Equal("Name", query.QueryParameters["p3"]);
            Assert.Equal("foo", query.QueryParameters["p4"]);
            Assert.Equal("Name", query.QueryParameters["p5"]);
            Assert.Equal("bar", query.QueryParameters["p6"]);
        }

        [Fact]
        public void IfThenElseVShouldAppendStepsWithThenQueryAndElseQueryWithParametersAndMultipleDeclarations()
        {
            var query = new NodeReference(123).IfThenElse(
                new GremlinIterator().OutV<Test>(t => t.Flag == true).GremlinHasNext(),
                new GremlinIterator().AggregateV<object>("x").OutV<Test>(t => t.Name == "foo"),
                new GremlinIterator().AggregateV<object>("y").InV<Test>(t => t.Name == "bar"));
            Assert.Equal("y = [];x = [];g.v(p0).ifThenElse{it.outV.filter{ it[p1] == p2 }.hasNext()}{it.aggregate(x).outV.filter{ it[p3].equalsIgnoreCase(p4) }}{it.aggregate(y).inV.filter{ it[p5].equalsIgnoreCase(p6) }}", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("Flag", query.QueryParameters["p1"]);
            Assert.Equal(true, query.QueryParameters["p2"]);
            Assert.Equal("Name", query.QueryParameters["p3"]);
            Assert.Equal("foo", query.QueryParameters["p4"]);
            Assert.Equal("Name", query.QueryParameters["p5"]);
            Assert.Equal("bar", query.QueryParameters["p6"]);
        }

        public class Test
        {
            public bool Flag { get; set; }
            public string Name { get; set; }
        }
    }
}