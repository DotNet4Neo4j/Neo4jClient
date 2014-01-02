using NUnit.Framework;
using NSubstitute;
using Neo4jClient.Cypher;
using System;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryConstraintTest
    {
        [Test]
        public void CreateUniqueConstraint()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .CreateConstraint("book:Book", "book.isbn")
                .Query;

            // Assert
            Assert.AreEqual("CREATE CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE", query.QueryText);
        }

        [Test]
        public void DropUniqueConstraint()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .DropConstraint("book:Book", "book.isbn")
                .Query;

            // Assert
            Assert.AreEqual("DROP CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE", query.QueryText);
        }
    }
}
