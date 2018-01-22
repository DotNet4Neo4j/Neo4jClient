using Xunit;
using NSubstitute;
using Neo4jClient.Cypher;
using System;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryRemoveTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void RemoveProperty()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Remove("n.age")
                .Return<Node<Object>>("n")
                .Query;

            // Assert
            Assert.Equal("START n=node({p0})\r\nREMOVE n.age\r\nRETURN n", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void RemoveLabel()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Remove("n:Person")
                .Return<Node<Object>>("n")
                .Query;

            // Assert
            Assert.Equal("START n=node({p0})\r\nREMOVE n:Person\r\nRETURN n", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }
    }
}
