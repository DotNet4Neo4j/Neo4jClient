using System;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherQueryTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void DebugQueryShouldBeSuccessfulWithNullAsParameters()
        {
            var query = new CypherQuery("MATCH (n) RETURN (n)", null, CypherResultMode.Set, "neo4j");

            const string expected = "MATCH (n) RETURN (n)";
            Assert.Equal(expected, query.DebugQueryText);
        }

        [Fact]
        public void DebugQueryTextShouldPreserveNewLines()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("foo")
                .CreateUnique("bar")
                .Query;

            string expected = "MATCH foo" + Environment.NewLine + "CREATE UNIQUE bar";
            Assert.Equal(expected, query.DebugQueryText);
        }

        [Theory]
        
        [InlineData("$param")]
        public void DebugQueryTextShouldSubstituteNumericParameters(string match)
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match(match)
                .WithParams(new
                {
                    param = 123
                })
                .Query;

            const string expected = "MATCH 123";
            Assert.Equal(expected, query.DebugQueryText);
        }

        [Theory]
        [InlineData("$param")]
        public void DebugQueryTextShouldSubstituteStringParametersWithEncoding(string match)
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match(match)
                .WithParams(new
                {
                    param = "hello"
                })
                .Query;

            const string expected = "MATCH \"hello\"";
            Assert.Equal(expected, query.DebugQueryText);
        }

        [Theory]
        [InlineData("$param")]
        public void DebugQueryTextShouldSubstituteStringParametersWithEncodingOfSpecialCharacters(string match)
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match(match)
                .WithParams(new
                {
                    param = "hel\"lo"
                })
                .Query;

            const string expected = "MATCH \"hel\\\"lo\"";
            Assert.Equal(expected, query.DebugQueryText);
        }

        [Theory]
        [InlineData("$param")]
        //[Description("https://github.com/Readify/Neo4jClient/issues/50")]
        public void DebugQueryTextShouldSubstituteNullParameters(string match)
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match(match)
                .WithParams(new
                {
                    param = (string)null
                })
                .Query;

            const string expected = "MATCH null";
            Assert.Equal(expected, query.DebugQueryText);
        }
    }
}