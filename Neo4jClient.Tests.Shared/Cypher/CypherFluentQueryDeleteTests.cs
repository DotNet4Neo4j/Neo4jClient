using System;
using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryDeleteTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void DeleteMatchedIdentifier()
        {
            // http://docs.neo4j.org/chunked/milestone/query-delete.html#delete-remove-a-node-and-connected-relationships
            // START n = node(3)
            // MATCH n-[r]-()
            // DELETE n, r

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Match("n-[r]-()")
                .Delete("n, r")
                .Query;

            Assert.Equal("START n=node({p0})\r\nMATCH n-[r]-()\r\nDELETE n, r", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void DeleteProperty()
        {
            // http://docs.neo4j.org/chunked/1.8.M06/query-delete.html#delete-remove-a-property
            //START andres = node(3)
            //DELETE andres.age
            //RETURN andres

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("andres", (NodeReference)3)
                .Delete("andres.age")
                .Return<Node<Object>>("andres")
                .Query;

            Assert.Equal("START andres=node({p0})\r\nDELETE andres.age\r\nRETURN andres", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void DeleteIdentifier()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Delete("n")
                .Query;

            Assert.Equal("START n=node({p0})\r\nDELETE n", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void DeleteWithoutReturn()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            CypherQuery executedQuery = null;
            client
                .When(c => c.ExecuteCypher(Arg.Any<CypherQuery>()))
                .Do(ci => { executedQuery = ci.Arg<CypherQuery>(); });

            // Act
            new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Delete("n")
                .ExecuteWithoutResults();

            // Assert
            Assert.NotNull(executedQuery);
            Assert.Equal("START n=node({p0})\r\nDELETE n", executedQuery.QueryText);
            Assert.Equal(3L, executedQuery.QueryParameters["p0"]);
        }

        [Fact]
        public void AllowDeleteClauseAfterWhere()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Where("(...)")
                .Delete("n")
                .Query;

            // Assert
            Assert.Equal("START n=node({p0})\r\nWHERE (...)\r\nDELETE n", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }
    }
}
