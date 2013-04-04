using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class AggregateTests
    {
        [Test]
        public void CountAll()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => new
                {
                    Foo = All.Count()
                })
                .Query;

            Assert.AreEqual("RETURN count(*) AS Foo", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
        }

        [Test]
        public void CountAllWithOtherIdentities()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(bar => new
                {
                    Foo = All.Count(),
                    Baz = bar.CollectAs<object>()
                })
                .Query;

            Assert.AreEqual("RETURN count(*) AS Foo, bar AS Baz", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
        }
    }
}
