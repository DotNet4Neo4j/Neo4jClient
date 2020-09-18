using System;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
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
                .Match("n")
                .Match("n-[r]-()")
                .Delete("n, r")
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "MATCH n-[r]-()" + Environment.NewLine + "DELETE n, r", query.QueryText);
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
                .Match("andres")
                .Delete("andres.age")
                .Return<Node<Object>>("andres")
                .Query;

            Assert.Equal("MATCH andres" + Environment.NewLine + "DELETE andres.age" + Environment.NewLine + "RETURN andres", query.QueryText);
        }

        [Fact]
        public void DeleteIdentifier()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Delete("n")
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "DELETE n", query.QueryText);
        }

        [Fact]
        public async Task DeleteWithoutReturn()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            CypherQuery executedQuery = null;
            client
                .When(c => c.ExecuteCypherAsync(Arg.Any<CypherQuery>()))
                .Do(ci => { executedQuery = ci.Arg<CypherQuery>(); });

            // Act
            await new CypherFluentQuery(client)
                .Match("n")
                .Delete("n")
                .ExecuteWithoutResultsAsync();

            // Assert
            Assert.NotNull(executedQuery);
            Assert.Equal("MATCH n" + Environment.NewLine + "DELETE n", executedQuery.QueryText);
        }

        [Fact]
        public void AllowDeleteClauseAfterWhere()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Where("(...)")
                .Delete("n")
                .Query;

            // Assert
            Assert.Equal("MATCH n" + Environment.NewLine + "WHERE (...)" + Environment.NewLine + "DELETE n", query.QueryText);
        }
    }
}
