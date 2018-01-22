using System;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.Cypher
{
    public class QueryWriterTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void AppendClause()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo");

            var query = writer.ToCypherQuery();
            Assert.Equal("foo", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void AppendClauseWithMultipleParameters()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0} bar {1}", "baz", "qak");

            var query = writer.ToCypherQuery();
            Assert.Equal("foo {p0} bar {p1}", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal("baz", query.QueryParameters["p0"]);
            Assert.Equal("qak", query.QueryParameters["p1"]);
        }

        [Fact]
        public void AppendClauseWithParameter()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0}", "bar");

            var query = writer.ToCypherQuery();
            Assert.Equal("foo {p0}", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal("bar", query.QueryParameters["p0"]);
        }

        [Fact]
        public void AppendMultipleClauses()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo");
            writer.AppendClause("bar");

            var query = writer.ToCypherQuery();
            Assert.Equal("foo\r\nbar", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void AppendMultipleClausesWithMultipleParameters()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0} bar {1}", "baz", "qak");
            writer.AppendClause("{0} qoo {1} zoo", "abc", "xyz");

            var query = writer.ToCypherQuery();
            var expectedText = string.Format("foo {{p0}} bar {{p1}}{0}{{p2}} qoo {{p3}} zoo", Environment.NewLine);
            Assert.Equal(expectedText, query.QueryText);
            Assert.Equal(4, query.QueryParameters.Count);
            Assert.Equal("baz", query.QueryParameters["p0"]);
            Assert.Equal("qak", query.QueryParameters["p1"]);
            Assert.Equal("abc", query.QueryParameters["p2"]);
            Assert.Equal("xyz", query.QueryParameters["p3"]);
        }

        [Fact]
        public void EmptyQueryForNoClauses()
        {
            var writer = new QueryWriter();

            var query = writer.ToCypherQuery();
            Assert.Equal("", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void ToCypherQueryShouldNotIncrementParamCountsWhenGeneratedTwice()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0}", "bar");

            var query1 = writer.ToCypherQuery();
            Assert.Equal("foo {p0}", query1.QueryText);
            Assert.Equal(1, query1.QueryParameters.Count);
            Assert.Equal("bar", query1.QueryParameters["p0"]);

            var query2 = writer.ToCypherQuery();
            Assert.Equal("foo {p0}", query2.QueryText);
            Assert.Equal(1, query2.QueryParameters.Count);
            Assert.Equal("bar", query2.QueryParameters["p0"]);
        }

        [Fact]
        public void ToCypherQueryShouldNotLeakNewParamsIntoPreviouslyBuiltQuery()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0}", "bar");
            var query1 = writer.ToCypherQuery();

            writer.AppendClause("baz {0}", "qak");
            var query2 = writer.ToCypherQuery();

            Assert.Equal("foo {p0}", query1.QueryText);
            Assert.Equal(1, query1.QueryParameters.Count);
            Assert.Equal("bar", query1.QueryParameters["p0"]);

            Assert.Equal("foo {p0}\r\nbaz {p1}", query2.QueryText);
            Assert.Equal(2, query2.QueryParameters.Count);
            Assert.Equal("bar", query2.QueryParameters["p0"]);
            Assert.Equal("qak", query2.QueryParameters["p1"]);
        }
    }
}