using Xunit;
using NSubstitute;
using Neo4jClient.Cypher;
using System;
using System.Linq;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQuerySkipTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SkipClause()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<Node<Object>>("n")
                .Skip(2)
                .Query;

            // Assert
            Assert.Equal("START n=node({p0})\r\nRETURN n\r\nSKIP {p1}", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(2, query.QueryParameters["p1"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void SkipClauseAfterReturnClauseIsTyped()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { n = new NodeReference(3) })
                .Return<Node<Object>>("n")
                .Skip(2);

            // Assert
            Assert.IsAssignableFrom<ICypherFluentQuery<Node<Object>>>(query);
        }

        [Fact]
        public void NullSkipDoesNotWriteClause()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<Node<Object>>("n")
                .Skip(null)
                .Query;

            // Assert
            Assert.Equal("START n=node({p0})\r\nRETURN n", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void SkipClauseAfterWithClause()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { n = new NodeReference(3) })
                .With("foo")
                .Skip(2)
                .Query;


            // Assert
            Assert.Equal("START n=node({p0})\r\nWITH foo\r\nSKIP {p1}", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(2, query.QueryParameters["p1"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void SkipClauseAfterWithClauseIsUntyped()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { n = new NodeReference(3) })
                .With("foo")
                .Skip(2);

            // Assert
            Assert.IsAssignableFrom<ICypherFluentQuery>(query);
            var implementsTypedQueryInterface = query
                .GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType)
                .Select(i => i.GetGenericTypeDefinition())
                .Any(t => t == typeof(ICypherFluentQuery<>));
            Assert.False(implementsTypedQueryInterface, "Implementes ICypherFluentQuery<>");
        }
    }
}
