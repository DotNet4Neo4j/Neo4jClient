using System;
using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    /// <summary>
    ///     Tests for the LOAD CSV command
    /// </summary>
    [TestFixture]
    public class CypherFluentQueryLoadCsvTests
    {
        [Test]
        public void TestLoadCsvConstruction()
        {
            const string expected = "LOAD CSV FROM 'file://localhost/c:/foo/bar.csv' AS row";
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .LoadCsv(new Uri("file://localhost/c:/foo/bar.csv"), "row")
                .Query;

            Assert.AreEqual(expected, query.QueryText);
        }

        [Test]
        public void TestLoadCsvAfterWithTResultVariant()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(n => new {n})
                .LoadCsv(new Uri("file://localhost/c:/foo/bar.csv"), "row")
                .Query;

            Assert.AreEqual("WITH n\r\nLOAD CSV FROM 'file://localhost/c:/foo/bar.csv' AS row", query.QueryText);
        }


        [Test]
        public void ThrowsExceptionWhenUriIsNull()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client);

            Assert.Throws<ArgumentException>(() => query.LoadCsv(null, "row"));
        }
    }
}
