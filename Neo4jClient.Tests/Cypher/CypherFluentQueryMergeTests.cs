using System;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
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
            Assert.Equal("MERGE (robert:Person)" + Environment.NewLine + "ON CREATE" + Environment.NewLine + "SET robert.Created = timestamp()", query.QueryText);
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
            Assert.Equal("MERGE (robert:Person)" + Environment.NewLine + "ON MATCH" + Environment.NewLine + "SET robert.LastSeen = timestamp()", query.QueryText);
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
            Assert.Equal("MERGE (robert:Person)" + Environment.NewLine + "ON CREATE" + Environment.NewLine + "SET robert.Created = timestamp()" + Environment.NewLine + "ON MATCH" + Environment.NewLine + "SET robert.LastSeen = timestamp()", query.QueryText);
        }
    }
}
