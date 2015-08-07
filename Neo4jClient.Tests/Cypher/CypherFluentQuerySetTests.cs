using NUnit.Framework;
using NSubstitute;
using Neo4jClient.Cypher;
using System;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQuerySetTests
    {
        [Test]
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
            Assert.AreEqual("START n=node({p0})\r\nSET n.age = 30\r\nRETURN n", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
        public void SetWithoutReturn()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference) 3)
                .Set("n.name = \"Ted\"")
                .Query;

            // Assert
            Assert.AreEqual("START n=node({p0})\r\nSET n.name = \"Ted\"", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }
    }
}
