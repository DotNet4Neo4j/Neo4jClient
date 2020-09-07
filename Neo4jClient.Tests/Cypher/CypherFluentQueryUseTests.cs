using System;
using FluentAssertions;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryUseTests : IClassFixture<CultureInfoSetupFixture>
    {
        // https://neo4j.com/docs/cypher-manual/current/clauses/use/
        [Fact]
        public void GeneratesTheCorrectCypher()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Use("neo4jclient")
                .Match("(n)")
                .Return<object>("n")
                .Query;

            query.QueryText.Should().Be($"USE neo4jclient{Environment.NewLine}MATCH (n){Environment.NewLine}RETURN n");
        }
    }
}