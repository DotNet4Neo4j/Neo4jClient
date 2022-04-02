using System;
using System.Collections.Generic;
using FluentAssertions;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherQueryTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        //Issue 436: https://github.com/DotNet4Neo4j/Neo4jClient/issues/436
        public void DebugQueryTextShouldSerializeObjectPropertyNamesWithoutQuotes()
        {
            var client = Substitute.For<IRawGraphClient>();
            var dummyObject = new { key = "value" };

            var query = new CypherFluentQuery(client)
                .Merge("(foo:Foo $object)")
                .WithParam("object", dummyObject)
                .Query;

            string expected = "MERGE (foo:Foo {" + Environment.NewLine + "  key: \"value\"" + Environment.NewLine + "})";
            Assert.Equal(expected, query.DebugQueryText);

            var dummyString = "value";

            query = new CypherFluentQuery(client)
                .Merge("(foo:Foo { key: $value })")
                .WithParam("value", dummyString)
                .Query;

            expected = "MERGE (foo:Foo { key: \"value\" })";
            Assert.Equal(expected, query.DebugQueryText);
        }

        [Fact]
        //Issue 349: https://github.com/DotNet4Neo4j/Neo4jClient/issues/349
        public void DebugQueryTextShouldWorkWithParametersWithTheSameStart()
        {
            var query = new CypherQuery("CREATE (n {a: $value, b: $value1, c: $value2})", new Dictionary<string, object> {{"value", "1"}, {"value1", "2"}, {"value2", "3"}}, CypherResultMode.Set, "neo4j");

            const string expected = "CREATE (n {a: \"1\", b: \"2\", c: \"3\"})";
            query.DebugQueryText.Should().Be(expected);
        }

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
            var text = query.QueryText;
            const string expected = "MATCH null";
            Assert.Equal(expected, query.DebugQueryText);
        }
    }
}