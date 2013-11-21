using NUnit.Framework;
using NSubstitute;
using Neo4jClient.Cypher;
using System.Linq;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryMatchTests
    {
        [Test]
        public void MatchRelatedNodes()
        {
            // http://docs.neo4j.org/chunked/1.6/query-match.html#match-related-nodes
            // START n=node(3)
            // MATCH (n)--(x)
            // RETURN x

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Match("(n)--(x)")
                .Return<object>("x")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nMATCH (n)--(x)\r\nRETURN x", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
        public void OptionalMatch()
        {
            // http://docs.neo4j.org/chunked/2.0.0-RC1/query-optional-match.html
            // OPTIONAL MATCH (n)--(x)
            // RETURN n, x

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .OptionalMatch("(n)--(x)")
                .Query;

            Assert.AreEqual("OPTIONAL MATCH (n)--(x)", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
        }

        [Test]
        public void MultipleMatchClauses()
        {
            // MATCH (n)
            // OPTIONAL MATCH (n)--(x)
            // OPTIONAL MATCH (x)--(a)
            // RETURN n, x

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(n)")
                .OptionalMatch("(n)--(x)")
                .OptionalMatch("(x)--(a)")
                .Query;

            Assert.AreEqual("MATCH (n)\r\nOPTIONAL MATCH (n)--(x)\r\nOPTIONAL MATCH (x)--(a)", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
        }

        [Test]
        public void MultipleMatchClausesWithPairedWhereClauses()
        {
            // MATCH (n)
            // WHERE n.Foo = {p0}
            // OPTIONAL MATCH (n)--(x)
            // WHERE x.Bar = {p1}
            // OPTIONAL MATCH (x)--(a)
            // WHERE a.Baz = {p2}
            // RETURN n, x

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(n)")
                .Where((FooBarBaz n) => n.Foo == "abc")
                .OptionalMatch("(n)--(x)")
                .Where((FooBarBaz x) => x.Bar == "def")
                .OptionalMatch("(x)--(a)")
                .Where((FooBarBaz a) => a.Baz == "ghi")
                .Query;

            const string expected =
                @"MATCH (n)
WHERE (n.Foo = {p0})
OPTIONAL MATCH (n)--(x)
WHERE (x.Bar = {p1})
OPTIONAL MATCH (x)--(a)
WHERE (a.Baz = {p2})";

            Assert.AreEqual(expected, query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count());
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        class FooBarBaz
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}
