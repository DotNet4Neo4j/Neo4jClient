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
                .Start("n", (NodeReference)3)
                .Set("n.age = 30")
                .Return<Node<Object>>("n")
                .Query;

            // Assert
            Assert.Equal("START n=node({p0})" + Environment.NewLine + "SET n.age = 30" + Environment.NewLine + "RETURN n", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void SetWithoutReturn()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference) 3)
                .Set("n.name = \"Ted\"")
                .Query;

            // Assert
            Assert.Equal("START n=node({p0})" + Environment.NewLine + "SET n.name = \"Ted\"", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }
    }
}
