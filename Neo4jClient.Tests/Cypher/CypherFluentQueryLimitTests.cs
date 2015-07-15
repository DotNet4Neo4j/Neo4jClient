using NUnit.Framework;
using NSubstitute;
using Neo4jClient.Cypher;
using System;
using System.Linq;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryLimitTests
    {
        [Test]
        public void LimitClause()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<Node<Object>>("n")
                .Limit(2)
                .Query;

            // Assert
            Assert.AreEqual("START n=node({p0})\r\nRETURN n\r\nLIMIT {p1}", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void LimitClauseAfterReturnClauseIsTyped()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { n = new NodeReference(3) })
                .Return<Node<Object>>("n")
                .Limit(2);

            // Assert
            Assert.IsInstanceOf<ICypherFluentQuery<Node<Object>>>(query);
        }

        [Test]
        public void NullLimitDoesNotWriteClause()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<Node<Object>>("n")
                .Limit(null)
                .Query;

            // Assert
            Assert.AreEqual("START n=node({p0})\r\nRETURN n", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void LimitClauseAfterWithClause()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { n = new NodeReference(3) })
                .With("foo")
                .Limit(2)
                .Query;


            // Assert
            Assert.AreEqual("START n=node({p0})\r\nWITH foo\r\nLIMIT {p1}", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void LimitClauseAfterWithClauseIsUntyped()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { n = new NodeReference(3) })
                .With("foo")
                .Limit(2);

            // Assert
            var implementsTypedQueryInterface = query
                .GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType)
                .Select(i => i.GetGenericTypeDefinition())
                .Any(t => t == typeof(ICypherFluentQuery<>));
            Assert.IsFalse(implementsTypedQueryInterface, "Implementes ICypherFluentQuery<>");
        }
    }
}
