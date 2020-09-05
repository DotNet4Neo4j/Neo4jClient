﻿using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQueryMaxExecutionTimeTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SetsMaxExecutionTime_WhenUsingAReturnTypeQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .MaxExecutionTime(100)
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.Equal(100, query.MaxExecutionTime);
        }

        [Fact]
        public void SetsMaxExecutionTime_WhenUsingANonReturnTypeQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .MaxExecutionTime(100)
                .Match("n")
                .Set("n.Value = 'value'")
                .Query;

            Assert.Equal(100, query.MaxExecutionTime);
        }

    }
}