using Xunit;
using NSubstitute;
using Neo4jClient.Cypher;
using System;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
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
