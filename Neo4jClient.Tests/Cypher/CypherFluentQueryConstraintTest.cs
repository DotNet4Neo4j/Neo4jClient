using System;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQueryConstraintTest : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void CreateUniqueConstraint()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .CreateUniqueConstraint("book:Book", "book.isbn")
                .Query;

            // Assert
            Assert.Equal("CREATE CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE", query.QueryText);
        }

        [Fact]
        public void DropUniqueConstraint()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .DropUniqueConstraint("book:Book", "book.isbn")
                .Query;

            // Assert
            Assert.Equal("DROP CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE", query.QueryText);
        }
    }
}
