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

            writer.AppendClause(
                "foo {0}",
                "bar");

            var query = writer.ToCypherQuery();
            Assert.AreEqual("foo {p0}", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual("bar", query.QueryParameters["p0"]);
        }

        [Test]
        public void AppendClauseWithMultipleParameters()
        {
            var writer = new QueryWriter();

            writer.AppendClause(
                "foo {0} bar {1}",
                "baz",
                "qak");

            var query = writer.ToCypherQuery();
            Assert.AreEqual("foo {p0} bar {p1}", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual("baz", query.QueryParameters["p0"]);
            Assert.AreEqual("qak", query.QueryParameters["p1"]);
        }
    }
}
