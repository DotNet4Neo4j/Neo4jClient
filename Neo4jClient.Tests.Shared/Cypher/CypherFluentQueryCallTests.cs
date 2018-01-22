using System;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryCallTests : IClassFixture<CultureInfoSetupFixture>
    {
        private static IRawGraphClient GraphClient_30
        {
            get
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(CypherCapabilities.Cypher30);
                return client;
            }
        }

        [Fact]
        public void CallsStoredProcedureGiven()
        {
            var client = GraphClient_30;
            var query = new CypherFluentQuery(client)
                .Call("apoc.sp()")
                .Query;

            Assert.Equal("CALL apoc.sp()", query.QueryText);
        }

        [Fact]
        public void ThrowArgumentException_WhenNoStoredProcedureIsGiven()
        {
            var client = GraphClient_30;
            Assert.Throws<ArgumentException>(() => new CypherFluentQuery(client).Call(null).Query);
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenClientVersionIsLessThan_30()
        {
            var client = GraphClient_30;
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);

            Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).Call("apoc.sp").Query);
        }
    }
}