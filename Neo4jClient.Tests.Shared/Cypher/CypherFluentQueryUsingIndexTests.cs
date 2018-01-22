using System;
using Xunit;
using NSubstitute;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryUsingIndexTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void UsesIndex()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(foo:Bar { id: 123 })")
                .UsingIndex(":Bar(id)")
                .Return(foo => new { qux = foo.As<object>() } )
                .Query;

            Assert.Equal("MATCH (foo:Bar { id: 123 })\r\nUSING INDEX :Bar(id)\r\nRETURN foo AS qux", query.QueryText);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void UsingEmptyIndexIsInvalid(string index)
        {
            var client = Substitute.For<IRawGraphClient>();
            Assert.Throws<ArgumentException>(() =>
            {
                var query = new CypherFluentQuery(client)
                    .Match("(foo:Bar { id: 123 })")
                    .UsingIndex(index)
                    .Return(foo => new {qux = foo.As<object>()})
                    .Query;
            });

        }
    }
}
