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
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local

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
    }
}
