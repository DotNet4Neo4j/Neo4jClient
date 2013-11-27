using NUnit.Framework;
using NSubstitute;
using Neo4jClient.Cypher;
using System;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryMergeTests
    {
        [Test]
        public void MergePropertyWithLabel()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Merge("(robert:Person)")
                .Query;

            // Assert
            Assert.AreEqual("MERGE (robert:Person)", query.QueryText);
        }

        [Test]
        public void MergeOnCreate()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Merge("(robert:Person)")
                .OnCreate()
                .Set("robert.Created = timestamp()")
                .Query;

            // Assert
            Assert.AreEqual("MERGE (robert:Person)\r\nON CREATE\r\nSET robert.Created = timestamp()", query.QueryText);
        }

        [Test]
        public void MergeOnMatch()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Merge("(robert:Person)")
                .OnMatch()
                .Set("robert.LastSeen = timestamp()")
                .Query;

            // Assert
            Assert.AreEqual("MERGE (robert:Person)\r\nON MATCH\r\nSET robert.LastSeen = timestamp()", query.QueryText);
        }

        [Test]
        public void MergeOnCreateOnMatch()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Merge("(robert:Person)")
                .OnCreate()
                .Set("robert.Created = timestamp()")
                .OnMatch()
                .Set("robert.LastSeen = timestamp()")
                .Query;

            // Assert
            Assert.AreEqual("MERGE (robert:Person)\r\nON CREATE\r\nSET robert.Created = timestamp()\r\nON MATCH\r\nSET robert.LastSeen = timestamp()", query.QueryText);
        }
    }
}
