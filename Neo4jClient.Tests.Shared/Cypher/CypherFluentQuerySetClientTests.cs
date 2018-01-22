using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQuerySetClientTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SetClient()
        {
            var client1 = Substitute.For<IRawGraphClient>();
            var client2 = Substitute.For<IRawGraphClient>();

            var query = new CypherFluentQuery(client1).Match("(n)");
            Assert.Same(((CypherFluentQuery) query).Client, client1);

            query = query.Advanced.SetClient(client2);
            Assert.Same(((CypherFluentQuery) query).Client, client2);
        }

        [Fact]
        public void SetClient_TResult()
        {
            var client1 = Substitute.For<IRawGraphClient>();
            var client2 = Substitute.For<IRawGraphClient>();

            var query = new CypherFluentQuery(client1).Match("(n)").Return(n => n.Count());
            Assert.Same(((CypherFluentQuery) query).Client, client1);

            query = query.Advanced.SetClient<long>(client2);
            Assert.Same(((CypherFluentQuery) query).Client, client2);
        }

        [Fact]
        public void JoinQueries()
        {
            var client1 = Substitute.For<IRawGraphClient>();
            var client2 = Substitute.For<IRawGraphClient>();

            var query = new CypherFluentQuery(client1).Match("(bar)");
            Assert.Same(((CypherFluentQuery)query).Client, client1);
            query = query.Advanced.SetClient(client2).Return(bar => new {Foo = bar.As<object>()});
            Assert.Same(((CypherFluentQuery)query).Client, client2);
        }
    }
}
