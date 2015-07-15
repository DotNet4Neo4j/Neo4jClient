using Newtonsoft.Json.Serialization;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

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
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        class MockWithNullField
        {
            public string NullField { get; set; }
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
