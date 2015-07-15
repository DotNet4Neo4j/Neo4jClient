using NUnit.Framework;
using NSubstitute;
using Neo4jClient.Cypher;
using System;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryRemoveTests
    {
        [Test]
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
            Assert.AreEqual("START n=node({p0})\r\nREMOVE n.age\r\nRETURN n", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
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
            Assert.AreEqual("START n=node({p0})\r\nREMOVE n:Person\r\nRETURN n", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }
    }
}
