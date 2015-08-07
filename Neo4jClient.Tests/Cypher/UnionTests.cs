using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class UnionTests
    {
        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-union.html#union-combine-two-queries-and-removing-duplicates")]
        public void UnionAll()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .UnionAll()
                .Query;

            Assert.AreEqual("UNION ALL", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
        }

        [Test]
        [Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-union.html#union-union-two-queries")]
        public void Union()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Union()
                .Query;

            Assert.AreEqual("UNION", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
        }
    }
}
