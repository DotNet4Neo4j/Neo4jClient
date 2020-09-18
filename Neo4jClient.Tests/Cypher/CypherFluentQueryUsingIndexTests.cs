using System;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
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

            Assert.Equal("MATCH (foo:Bar { id: 123 })" + Environment.NewLine + "USING INDEX :Bar(id)" + Environment.NewLine + "RETURN foo AS qux", query.QueryText);
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
