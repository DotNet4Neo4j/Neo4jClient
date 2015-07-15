using System;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryDropTests
    {
        [Test]
        public void DropIndex()
        {
            // http://docs.neo4j.org/chunked/milestone/query-schema-index.html#schema-index-drop-index-on-a-label
            // DROP INDEX ON :Person(name)

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Drop("INDEX ON :Person(name)")
                .Query;

            Assert.AreEqual("DROP INDEX ON :Person(name)", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
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

            Assert.AreEqual("START andres=node({p0})\r\nDELETE andres.age\r\nRETURN andres", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
        public void DeleteIdentifier()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Delete("n")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nDELETE n", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
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
            Assert.IsNotNull(executedQuery, "Query was not executed against graph client");
            Assert.AreEqual("START n=node({p0})\r\nDELETE n", executedQuery.QueryText);
            Assert.AreEqual(3, executedQuery.QueryParameters["p0"]);
        }

        [Test]
        public void AllowDeleteClauseAfterWhere()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Where("(...)")
                .Delete("n")
                .Query;

            // Assert
            Assert.AreEqual("START n=node({p0})\r\nWHERE (...)\r\nDELETE n", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }
    }
}
