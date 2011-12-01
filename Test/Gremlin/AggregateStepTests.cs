using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class AggregateStepTests
    {
        [Test]
        public void AggregateVShouldAppendStep()
        {
            var query = new NodeReference(123).AggregateV<object>("foo");
            Assert.AreEqual("p2 = [];g.v(p0).aggregate(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("foo", query.QueryParameters["p2"]);
        }

        [Test]
        public void AggregateVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).AggregateV<object>("foo");
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
        }

        [Test]
        public void AggregateEShouldAppendStep()
        {
            var query = new NodeReference(123).AggregateE("foo");
            Assert.AreEqual("p2 = [];g.v(p0).aggregate(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("foo", query.QueryParameters["p2"]);
        }

        [Test]
        public void AggregateEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).AggregateE("foo");
            Assert.IsInstanceOf<GremlinRelationshipEnumerable>(query);
        }

        [Test]
        public void AggregateEWithTDataShouldAppendStep()
        {
            var query = new NodeReference(123).AggregateE<object>("foo");
            Assert.AreEqual("p2 = [];g.v(p0).aggregate(p1)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual("foo", query.QueryParameters["p1"]);
            Assert.AreEqual("foo", query.QueryParameters["p2"]);
        }

        [Test]
        public void AggregateEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).AggregateE<object>("foo");
            Assert.IsInstanceOf<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}