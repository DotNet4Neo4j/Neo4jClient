using System;
using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryDetachDeleteTests
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

        [Test]
        public void DeleteMatchedIdentifier()
        {
            var client = GraphClient_230;
            var query = new CypherFluentQuery(client)
                .Match("n-[r]-()")
                .DetachDelete("n, r")
                .Query;

            Assert.AreEqual("MATCH n-[r]-()\r\nDETACH DELETE n, r", query.QueryText);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThrowInvalidOperationException_WhenAttemptingToDeleteProperty()
        {
            var client = GraphClient_230;
            var query = new CypherFluentQuery(client)
                .DetachDelete("andres.age")
                .Return<Node<object>>("andres")
                .Query;
        }

        [Test]
        public void DeleteIdentifier()
        {
            var client = GraphClient_230;
            var query = new CypherFluentQuery(client)
                .Match("n")
                .DetachDelete("n")
                .Query;

            Assert.AreEqual("MATCH n\r\nDETACH DELETE n", query.QueryText);
        }

        [Test]
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
            Assert.IsNotNull(executedQuery, "Query was not executed against graph client");
            Assert.AreEqual("DETACH DELETE n", executedQuery.QueryText);
        }

        [Test]
        public void AllowDeleteClauseAfterWhere()
        {
            var client = GraphClient_230;
            var query = new CypherFluentQuery(client)
                .Where("(...)")
                .DetachDelete("n")
                .Query;

            // Assert
            Assert.AreEqual("WHERE (...)\r\nDETACH DELETE n", query.QueryText);
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void ThrowsInvalidOperationException_WhenClientVersionIsLessThan_230()
        {
            var client = GraphClient_230;
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher226);
            
            var query = new CypherFluentQuery(client)
                .Match("(n)")
                .DetachDelete("n")
                .Query;
        }
    }
}