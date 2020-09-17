using System;
using System.Linq;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQuerySkipTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SkipClause()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Return<Node<Object>>("n")
                .Skip(2)
                .Query;

            // Assert
            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN n" + Environment.NewLine + "SKIP $p0", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(2, query.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void SkipClauseAfterReturnClauseIsTyped()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
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
                .Match("n")
                .Return<Node<Object>>("n")
                .Skip(null)
                .Query;

            // Assert
            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void SkipClauseAfterWithClause()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .With("foo")
                .Skip(2)
                .Query;


            // Assert
            Assert.Equal("MATCH n" + Environment.NewLine + "WITH foo" + Environment.NewLine + "SKIP $p0", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(2, query.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/140/support-multiple-limit-order-by-clauses-in")]
        public void SkipClauseAfterWithClauseIsUntyped()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
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
