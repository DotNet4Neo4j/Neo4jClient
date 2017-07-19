using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQuerySetClientTests
    {
        [Test]
        public void SetClient()
        {
            var client1 = Substitute.For<IRawGraphClient>();
            var client2 = Substitute.For<IRawGraphClient>();

            var query = new CypherFluentQuery(client1).Match("(n)");
            Assert.AreSame(((CypherFluentQuery) query).Client, client1);

            query = query.Advanced.SetClient(client2);
            Assert.AreSame(((CypherFluentQuery) query).Client, client2);
        }

        [Test]
        public void SetClient_TResult()
        {
            var client1 = Substitute.For<IRawGraphClient>();
            var client2 = Substitute.For<IRawGraphClient>();

            var query = new CypherFluentQuery(client1).Match("(n)").Return(n => n.Count());
            Assert.AreSame(((CypherFluentQuery) query).Client, client1);

            query = query.Advanced.SetClient<long>(client2);
            Assert.AreSame(((CypherFluentQuery) query).Client, client2);
        }

        [Test]
        public void JoinQueries()
        {
            var client1 = Substitute.For<IRawGraphClient>();
            var client2 = Substitute.For<IRawGraphClient>();

            var query = new CypherFluentQuery(client1).Match("(bar)");
            Assert.AreSame(((CypherFluentQuery)query).Client, client1);
            query = query.Advanced.SetClient(client2).Return(bar => new {Foo = bar.As<object>()});
            Assert.AreSame(((CypherFluentQuery)query).Client, client2);
        }
    }
}