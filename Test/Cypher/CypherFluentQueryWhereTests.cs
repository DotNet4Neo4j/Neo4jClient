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
    }
}
