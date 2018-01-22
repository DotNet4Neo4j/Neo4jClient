using Xunit;
using NSubstitute;
using Neo4jClient.Cypher;
using System;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryMergeTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void MergePropertyWithLabel()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Merge("(robert:Person)")
                .Query;

            // Assert
            Assert.Equal("MERGE (robert:Person)", query.QueryText);
        }

        [Fact]
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
            Assert.Equal("MERGE (robert:Person)\r\nON CREATE\r\nSET robert.Created = timestamp()", query.QueryText);
        }

        [Fact]
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
            Assert.Equal("MERGE (robert:Person)\r\nON MATCH\r\nSET robert.LastSeen = timestamp()", query.QueryText);
        }

        [Fact]
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
            Assert.Equal("MERGE (robert:Person)\r\nON CREATE\r\nSET robert.Created = timestamp()\r\nON MATCH\r\nSET robert.LastSeen = timestamp()", query.QueryText);
        }
    }
}
