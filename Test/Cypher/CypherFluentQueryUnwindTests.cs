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
    }
}
