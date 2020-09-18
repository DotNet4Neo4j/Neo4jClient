using System;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQuerySetTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SetProperty()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Set("n.age = 30")
                .Return<Node<Object>>("n")
                .Query;

            // Assert
            Assert.Equal("MATCH n" + Environment.NewLine + "SET n.age = 30" + Environment.NewLine + "RETURN n", query.QueryText);
        }

        [Fact]
        public void SetWithoutReturn()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Set("n.name = \"Ted\"")
                .Query;

            // Assert
            Assert.Equal("MATCH n" + Environment.NewLine + "SET n.name = \"Ted\"", query.QueryText);
        }
    }
}
