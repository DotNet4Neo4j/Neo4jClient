using System;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryWhereTests
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

        class MockWithNullField
        {
            public string NullField { get; set; }
        }
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local


        [Test]
        public void CreatesStartWithQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);
            const string startsWith = "Bar";
            var query = new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar.StartsWith(startsWith)).Query;

            Assert.AreEqual("WHERE (foo.bar STARTS WITH {p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(startsWith, query.QueryParameters["p0"]);
        }

        [Test]
        public void CreatesEndsWithQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);
            const string startsWith = "Bar";
            var query = new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar.EndsWith(startsWith)).Query;

            Assert.AreEqual("WHERE (foo.bar ENDS WITH {p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(startsWith, query.QueryParameters["p0"]);
        }

        [Test]
        public void CreatesContainsQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);
            const string startsWith = "Bar";
            var query = new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar.Contains(startsWith)).Query;

            Assert.AreEqual("WHERE (foo.bar CONTAINS {p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(startsWith, query.QueryParameters["p0"]);
        }

        [Test]
        public void ThrowsNotSupportedException_WhenNeo4jInstanceIsLowerThan23()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher22);
            const string startsWith = "Bar";
            Assert.Throws<NotSupportedException>(() => new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar.StartsWith(startsWith)));
        }

        [Test]
        public void UsesJsonPropertyNameOverPropertyName()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client).Where((FooWithJsonProperties foo) => foo.Bar == "Bar").Query;

            Assert.AreEqual("WHERE (foo.bar = {p0})", query.QueryText);
        }

        [Test]
        public void ComparePropertiesAcrossEntitiesEqual()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<Foo, Foo>((a, b) => a.Bar == b.Bar)
                .Query;

            Assert.AreEqual("WHERE (a.Bar = b.Bar)", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void ComparePropertiesAcrossEntitiesNotEqual()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<Foo, Foo>((a, b) => a.Bar != b.Bar)
                .Query;

            Assert.AreEqual("WHERE (a.Bar <> b.Bar)", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void NestOrAndAndCorrectly()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where((Foo a, Foo b) => a.Bar == 123 || b.Bar == 456)
                .AndWhere((Foo c) => c.Bar == 789)
                .Query;

            Assert.AreEqual("WHERE ((a.Bar = {p0}) OR (b.Bar = {p1}))\r\nAND (c.Bar = {p2})", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
        }

        [Test]
        public void ComparePropertiesAcrossEntitiesEqualCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Where<FooCamel, FooCamel>((a, b) => a.Bar == b.Bar && a.LongBar == b.LongBar && a.a == b.a && a.B == b.B)
                .Query;

            Assert.AreEqual("WHERE ((((a.bar = b.bar) AND (a.longBar = b.longBar)) AND (a.a = b.a)) AND (a.b = b.b))", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void ComparePropertiesAcrossEntitiesNotEqualCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Where<FooCamel, FooCamel>((a, b) => a.Bar != b.Bar && a.LongBar != b.LongBar && a.a != b.a && a.B != b.B)
                .Query;

            Assert.AreEqual("WHERE ((((a.bar <> b.bar) AND (a.longBar <> b.longBar)) AND (a.a <> b.a)) AND (a.b <> b.b))", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void NestOrAndAndCorrectlyCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Where((FooCamel a, FooCamel b) => a.LongBar == 123 || b.Bar == 456)
                .AndWhere((FooCamel c) => c.B == 789)
                .Query;

            Assert.AreEqual("WHERE ((a.longBar = {p0}) OR (b.bar = {p1}))\r\nAND (c.b = {p2})", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
        }
    }
}
