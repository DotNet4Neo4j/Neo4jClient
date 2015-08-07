using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherQueryTests
    {
        [Test]
        public void DebugQueryTextShouldPreserveNewLines()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("foo")
                .CreateUnique("bar")
                .Query;

            const string expected = "MATCH foo\r\nCREATE UNIQUE bar";
            Assert.AreEqual(expected, query.DebugQueryText);
        }

        [Test]
        public void DebugQueryTextShouldSubstituteNumericParameters()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("{param}")
                .WithParams(new
                {
                    param = 123
                })
                .Query;

            const string expected = "MATCH 123";
            Assert.AreEqual(expected, query.DebugQueryText);
        }

        [Test]
        public void DebugQueryTextShouldSubstituteStringParametersWithEncoding()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("{param}")
                .WithParams(new
                {
                    param = "hello"
                })
                .Query;

            const string expected = "MATCH \"hello\"";
            Assert.AreEqual(expected, query.DebugQueryText);
        }

        [Test]
        public void DebugQueryTextShouldSubstituteStringParametersWithEncodingOfSpecialCharacters()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("{param}")
                .WithParams(new
                {
                    param = "hel\"lo"
                })
                .Query;

            const string expected = "MATCH \"hel\\\"lo\"";
            Assert.AreEqual(expected, query.DebugQueryText);
        }

        [Test]
        [Description("https://github.com/Readify/Neo4jClient/issues/50")]
        public void DebugQueryTextShouldSubstituteNullParameters()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("{param}")
                .WithParams(new
                {
                    param = (string)null
                })
                .Query;

            const string expected = "MATCH null";
            Assert.AreEqual(expected, query.DebugQueryText);
        }
    }
}
