using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class QueryWriterTests
    {
        [Test]
        public void EmptyQueryForNoClauses()
        {
            var writer = new QueryWriter();

            var query = writer.ToCypherQuery();
            Assert.AreEqual("", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void AppendClause()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo");

            var query = writer.ToCypherQuery();
            Assert.AreEqual("foo", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void AppendMultipleClauses()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo");
            writer.AppendClause("bar");

            var query = writer.ToCypherQuery();
            Assert.AreEqual("foo\r\nbar", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void AppendClauseWithParameter()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0}", "bar");

            var query = writer.ToCypherQuery();
            Assert.AreEqual("foo {p0}", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual("bar", query.QueryParameters["p0"]);
        }

        [Test]
        public void ToCypherQueryShouldNotIncrementParamCountsWhenGeneratedTwice()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0}", "bar");

            var query1 = writer.ToCypherQuery();
            Assert.AreEqual("foo {p0}", query1.QueryText);
            Assert.AreEqual(1, query1.QueryParameters.Count);
            Assert.AreEqual("bar", query1.QueryParameters["p0"]);

            var query2 = writer.ToCypherQuery();
            Assert.AreEqual("foo {p0}", query2.QueryText);
            Assert.AreEqual(1, query2.QueryParameters.Count);
            Assert.AreEqual("bar", query2.QueryParameters["p0"]);
        }

        [Test]
        public void ToCypherQueryShouldNotLeakNewParamsIntoPreviouslyBuiltQuery()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0}", "bar");
            var query1 = writer.ToCypherQuery();

            writer.AppendClause("baz {0}", "qak");
            var query2 = writer.ToCypherQuery();

            Assert.AreEqual("foo {p0}", query1.QueryText);
            Assert.AreEqual(1, query1.QueryParameters.Count);
            Assert.AreEqual("bar", query1.QueryParameters["p0"]);

            Assert.AreEqual("foo {p0}\r\nbaz {p1}", query2.QueryText);
            Assert.AreEqual(2, query2.QueryParameters.Count);
            Assert.AreEqual("bar", query2.QueryParameters["p0"]);
            Assert.AreEqual("qak", query2.QueryParameters["p1"]);
        }

        [Test]
        public void AppendClauseWithMultipleParameters()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0} bar {1}", "baz", "qak");

            var query = writer.ToCypherQuery();
            Assert.AreEqual("foo {p0} bar {p1}", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual("baz", query.QueryParameters["p0"]);
            Assert.AreEqual("qak", query.QueryParameters["p1"]);
        }

        [Test]
        public void AppendMultipleClausesWithMultipleParameters()
        {
            var writer = new QueryWriter();

            writer.AppendClause("foo {0} bar {1}", "baz", "qak");
            writer.AppendClause("{0} qoo {1} zoo", "abc", "xyz");

            var query = writer.ToCypherQuery();
            const string expectedText = @"foo {p0} bar {p1}
{p2} qoo {p3} zoo";
            Assert.AreEqual(expectedText, query.QueryText);
            Assert.AreEqual(4, query.QueryParameters.Count);
            Assert.AreEqual("baz", query.QueryParameters["p0"]);
            Assert.AreEqual("qak", query.QueryParameters["p1"]);
            Assert.AreEqual("abc", query.QueryParameters["p2"]);
            Assert.AreEqual("xyz", query.QueryParameters["p3"]);
        }
    }
}
