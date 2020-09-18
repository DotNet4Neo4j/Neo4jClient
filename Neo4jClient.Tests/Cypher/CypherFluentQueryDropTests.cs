using System;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQueryDropTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void DropIndex()
        {
            // http://docs.neo4j.org/chunked/milestone/query-schema-index.html#schema-index-drop-index-on-a-label
            // DROP INDEX ON :Person(name)

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Drop("INDEX ON :Person(name)")
                .Query;

            Assert.Equal("DROP INDEX ON :Person(name)", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void DeleteProperty()
        {
            // http://docs.neo4j.org/chunked/1.8.M06/query-delete.html#delete-remove-a-property
            //MATCH andres = node(3)
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
