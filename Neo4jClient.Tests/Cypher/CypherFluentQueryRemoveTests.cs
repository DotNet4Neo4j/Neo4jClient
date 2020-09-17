using System;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQueryRemoveTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void RemoveProperty()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Remove("n.age")
                .Return<Node<Object>>("n")
                .Query;

            // Assert
            Assert.Equal("MATCH n" + Environment.NewLine + "REMOVE n.age" + Environment.NewLine + "RETURN n", query.QueryText);
        }

        [Fact]
        public void RemoveLabel()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Remove("n:Person")
                .Return<Node<Object>>("n")
                .Query;

            // Assert
            Assert.Equal("MATCH n" + Environment.NewLine + "REMOVE n:Person" + Environment.NewLine + "RETURN n", query.QueryText);
        }
    }
}
