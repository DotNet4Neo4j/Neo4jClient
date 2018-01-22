using Xunit;
using NSubstitute;
using Neo4jClient.Cypher;
using System;
using System.Linq;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryLimitTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
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
            Assert.Equal("START n=node({p0})\r\nRETURN n\r\nLIMIT {p1}", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(2, query.QueryParameters["p1"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void LimitClauseAfterReturnClauseIsTyped()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { n = new NodeReference(3) })
                .Return<Node<Object>>("n")
                .Limit(2);

            // Assert
            Assert.IsAssignableFrom<ICypherFluentQuery<Node<Object>>>(query);
        }

        [Fact]
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
            Assert.Equal("START n=node({p0})\r\nRETURN n", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
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
            Assert.Equal("START n=node({p0})\r\nWITH foo\r\nLIMIT {p1}", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(2, query.QueryParameters["p1"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
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
            Assert.False(implementsTypedQueryInterface, "Implementes ICypherFluentQuery<>");
        }
    }
}
