using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryForEachTests
    {
        [Test]
        public void ForEachRawText()
        {
            // http://docs.neo4j.org/chunked/milestone/query-foreach.html
            // FOREACH (n IN nodes(p) | SET n.marked = TRUE)

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ForEach("(n IN nodes(p) | SET n.marked = TRUE)")
                .Query;

            Assert.AreEqual("FOREACH (n IN nodes(p) | SET n.marked = TRUE)", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }
    }
}
