using System;
using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    public class CypherFluentQueryParserVersionTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SetsVersionToFreeTextGiven()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion("2.1.experimental")
                .Start(new
                {
                    n = All.Nodes,
                })
                .Return<object>("n")
                .Query;

            Assert.Equal("CYPHER 2.1.experimental\r\nSTART n=node(*)\r\nRETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);   
        }

        [Fact]
        public void SetsVersion_WhenUsingVersionOverload()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(new Version(1, 9))
                .Start(new
                {
                    n = All.Nodes,
                })
                .Return<object>("n")
                .Query;

            Assert.Equal("CYPHER 1.9\r\nSTART n=node(*)\r\nRETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void SetsVersion_WhenUsingIntOverload()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(1, 9)
                .Start(new
                {
                    n = All.Nodes,
                })
                .Return<object>("n")
                .Query;

            Assert.Equal("CYPHER 1.9\r\nSTART n=node(*)\r\nRETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void UsesLegacy_WhenVersionRequestedIsLessThan1_9()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(1, 8)
                .Start(new
                {
                    n = All.Nodes,
                })
                .Return<object>("n")
                .Query;

            Assert.Equal("CYPHER LEGACY\r\nSTART n=node(*)\r\nRETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);            
        }
    }
}
