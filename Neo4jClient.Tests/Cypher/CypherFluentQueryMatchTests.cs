using System;
using System.Linq;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryMatchTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void MatchRelatedNodes()
        {
            // http://docs.neo4j.org/chunked/1.6/query-match.html#match-related-nodes
            // MATCH n
            // MATCH (n)--(x)
            // RETURN x

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Match("(n)--(x)")
                .Return<object>("x")
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "MATCH (n)--(x)" + Environment.NewLine + "RETURN x", query.QueryText);
        }

        [Fact]
        public void OptionalMatch()
        {
            // http://docs.neo4j.org/chunked/2.0.0-RC1/query-optional-match.html
            // OPTIONAL MATCH (n)--(x)
            // RETURN n, x

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .OptionalMatch("(n)--(x)")
                .Query;

            Assert.Equal("OPTIONAL MATCH (n)--(x)", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
        }

        [Fact]
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

            Assert.Equal("MATCH (n)" + Environment.NewLine + "OPTIONAL MATCH (n)--(x)" + Environment.NewLine + "OPTIONAL MATCH (x)--(a)", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count());
        }

        [Fact]
        public void MultipleMatchClausesWithPairedWhereClauses()
        {
            // MATCH (n)
            // WHERE n.Foo = $p0
            // OPTIONAL MATCH (n)--(x)
            // WHERE x.Bar = $p1
            // OPTIONAL MATCH (x)--(a)
            // WHERE a.Baz = $p2
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

            string expected = "MATCH (n)" + Environment.NewLine + "WHERE (n.Foo = $p0)" + Environment.NewLine + "OPTIONAL MATCH (n)--(x)" + Environment.NewLine + "WHERE (x.Bar = $p1)" + Environment.NewLine + "OPTIONAL MATCH (x)--(a)" + Environment.NewLine + "WHERE (a.Baz = $p2)";

            Assert.Equal(expected, query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count());
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
