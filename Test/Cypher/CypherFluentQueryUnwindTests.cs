using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    /// <summary>
    ///     Tests for the UNWIND operator
    /// </summary>
    [TestFixture]
    public class CypherFluentQueryUnwindTests
    {
        [Test]
        public void TestUnwindConstruction()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Unwind("collection", "column")
                .Query;

            Assert.AreEqual("UNWIND collection AS column", query.QueryText);
        }

        [Test]
        public void TestUnwindAfterWithTResultVariant()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(collection => new { collection })
                .Unwind("collection", "column")
                .Query;

            Assert.AreEqual("WITH collection\r\nUNWIND collection AS column", query.QueryText);
        }

        [Test]
        public void TestUnwindUsingCollection()
        {
            var collection = new[] { 1, 2, 3 };
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Unwind(collection, "alias")
                .Query;

            Assert.AreEqual("UNWIND {p0} AS alias", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(collection, query.QueryParameters["p0"]);
        }
    }
}
