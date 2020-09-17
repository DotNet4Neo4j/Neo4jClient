using System;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryParserVersionTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SetsVersionToFreeTextGiven()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion("2.1.experimental")
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.Equal("CYPHER 2.1.experimental" + Environment.NewLine + "MATCH n" + Environment.NewLine + "RETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);   
        }

        [Fact]
        public void SetsVersion_WhenUsingVersionOverload()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(new Version(1, 9))
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.Equal("CYPHER 1.9" + Environment.NewLine + "MATCH n" + Environment.NewLine + "RETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void SetsVersion_WhenUsingIntOverload()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(1, 9)
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.Equal("CYPHER 1.9" + Environment.NewLine + "MATCH n" + Environment.NewLine + "RETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void UsesLegacy_WhenVersionRequestedIsLessThan1_9()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .ParserVersion(1, 8)
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.Equal("CYPHER LEGACY" + Environment.NewLine + "MATCH n" + Environment.NewLine + "RETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);            
        }
    }
}
