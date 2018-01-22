using System;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryWhereTests : IClassFixture<CultureInfoSetupFixture>
    {
        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        class Foo
        {
            public int Bar { get; set; }
        }

        class FooCamel:Foo
        {
            public int LongBar { get; set; }
            public int a { get; set; }
            public int B { get; set; }
        }

        class FooWithJsonProperties
        {
            [JsonProperty("bar")]
            public string Bar { get; set; }
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        class FooWithCamelCaseNamingStrategy
        {
            public string Bar { get; set; }
        }

        class MockWithNullField
        {
            public string NullField { get; set; }
        }
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local


        [Fact]
        public void CreatesStartWithQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);
            const string startsWith = "Bar";
            var query = new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar.StartsWith(startsWith)).Query;

            Assert.Equal("WHERE (foo.bar STARTS WITH {p0})", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(startsWith, query.QueryParameters["p0"]);
        }

 		[Fact]
        public void CreatesEndsWithQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);
            const string endsWith = "Bar";
            var query = new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar.EndsWith(endsWith)).Query;

            Assert.Equal("WHERE (foo.bar ENDS WITH {p0})", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(endsWith, query.QueryParameters["p0"]);
        }

        [Fact]
        public void CreatesContainsQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);
            const string contains = "Bar";
            var query = new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar.Contains(contains)).Query;

            Assert.Equal("WHERE (foo.bar CONTAINS {p0})", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(contains, query.QueryParameters["p0"]);
        }

        [Fact]
        public void ThrowsNotSupportedException_WhenNeo4jInstanceIsLowerThan23()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher22);
            const string startsWith = "Bar";
            Assert.Throws<NotSupportedException>(() => new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar.StartsWith(startsWith)));
        }

        [Fact]
        public void UsesJsonPropertyNameOverPropertyName()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar == "Bar").Query;

            Assert.Equal("WHERE (foo.bar = {p0})", query.QueryText);
        }
		
		[Fact]
        public void UsesCamelCaseNamingStrategyOverPropertyName()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client).Where((FooWithCamelCaseNamingStrategy foo) => foo.Bar == "Bar").Query;

            Assert.Equal("WHERE (foo.bar = {p0})", query.QueryText);
        }

        [Fact]
        public void ComparePropertiesAcrossEntitiesEqual()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<Foo, Foo>((a, b) => a.Bar == b.Bar)
                .Query;

            Assert.Equal("WHERE (a.Bar = b.Bar)", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void ComparePropertiesAcrossEntitiesNotEqual()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<Foo, Foo>((a, b) => a.Bar != b.Bar)
                .Query;

            Assert.Equal("WHERE (a.Bar <> b.Bar)", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void NestOrAndAndCorrectly()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where((Foo a, Foo b) => a.Bar == 123 || b.Bar == 456)
                .AndWhere((Foo c) => c.Bar == 789)
                .Query;

            Assert.Equal("WHERE ((a.Bar = {p0}) OR (b.Bar = {p1}))\r\nAND (c.Bar = {p2})", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
        }

        [Fact]
        public void ComparePropertiesAcrossEntitiesEqualCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Where<FooCamel, FooCamel>((a, b) => a.Bar == b.Bar && a.LongBar == b.LongBar && a.a == b.a && a.B == b.B)
                .Query;

            Assert.Equal("WHERE ((((a.bar = b.bar) AND (a.longBar = b.longBar)) AND (a.a = b.a)) AND (a.b = b.b))", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void ComparePropertiesAcrossEntitiesNotEqualCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Where<FooCamel, FooCamel>((a, b) => a.Bar != b.Bar && a.LongBar != b.LongBar && a.a != b.a && a.B != b.B)
                .Query;

            Assert.Equal("WHERE ((((a.bar <> b.bar) AND (a.longBar <> b.longBar)) AND (a.a <> b.a)) AND (a.b <> b.b))", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void NestOrAndAndCorrectlyCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Where((FooCamel a, FooCamel b) => a.LongBar == 123 || b.Bar == 456)
                .AndWhere((FooCamel c) => c.B == 789)
                .Query;

            Assert.Equal("WHERE ((a.longBar = {p0}) OR (b.bar = {p1}))\r\nAND (c.b = {p2})", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
        }
    }
}
