using System;
using FluentAssertions;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQueryReadWriteTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SetsIsWriteToTrueWhenUsingWriteProperty()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Write
                .Create("(n:Foo)")
                .Query;

            Assert.Equal("CREATE (n:Foo)", query.QueryText);
            Assert.True(query.IsWrite);
        }

        [Fact]
        public void SetsIsWriteToTrueByDefault()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Create("(n:Foo)")
                .Query;

            Assert.Equal("CREATE (n:Foo)", query.QueryText);
            Assert.True(query.IsWrite);
        }

        [Fact]
        public void SetsIsWriteToFalseWhenUsingWriteProperty()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Read
                .Match("(n:Foo)")
                .Query;

            Assert.Equal("MATCH (n:Foo)", query.QueryText);
            Assert.False(query.IsWrite);
        }
    }
}
