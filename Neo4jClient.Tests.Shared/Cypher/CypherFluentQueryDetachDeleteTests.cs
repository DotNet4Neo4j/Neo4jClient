using System;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryDetachDeleteTests : IClassFixture<CultureInfoSetupFixture>
    {
        private static IRawGraphClient GraphClient_230
        {
            get
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);
                return client;
            }
        }

        [Fact]
        public void DeleteMatchedIdentifier()
        {
            var client = GraphClient_230;
            var query = new CypherFluentQuery(client)
                .Match("n-[r]-()")
                .DetachDelete("n, r")
                .Query;

            Assert.Equal("MATCH n-[r]-()\r\nDETACH DELETE n, r", query.QueryText);
        }

        [Fact]
        public void ThrowInvalidOperationException_WhenAttemptingToDeleteProperty()
        {
            var client = GraphClient_230;
            Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client)
                .DetachDelete("andres.age")
                .Return<Node<object>>("andres")
                .Query);
        }

        [Fact]
        public void DeleteIdentifier()
        {
            var client = GraphClient_230;
            var query = new CypherFluentQuery(client)
                .Match("n")
                .DetachDelete("n")
                .Query;

            Assert.Equal("MATCH n\r\nDETACH DELETE n", query.QueryText);
        }

        [Fact]
        public void DeleteWithoutReturn()
        {
            // Arrange
            var client = GraphClient_230;
            CypherQuery executedQuery = null;
            client
                .When(c => c.ExecuteCypher(Arg.Any<CypherQuery>()))
                .Do(ci => { executedQuery = ci.Arg<CypherQuery>(); });

            // Act
            new CypherFluentQuery(client)
                .DetachDelete("n")
                .ExecuteWithoutResults();

            // Assert
            Assert.NotNull(executedQuery);
            Assert.Equal("DETACH DELETE n", executedQuery.QueryText);
        }

        [Fact]
        public void AllowDeleteClauseAfterWhere()
        {
            var client = GraphClient_230;
            var query = new CypherFluentQuery(client)
                .Where("(...)")
                .DetachDelete("n")
                .Query;

            // Assert
            Assert.Equal("WHERE (...)\r\nDETACH DELETE n", query.QueryText);
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenClientVersionIsLessThan_230()
        {
            var client = GraphClient_230;
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher226);

            Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client)
                .Match("(n)")
                .DetachDelete("n")
                .Query);
        }
    }
}