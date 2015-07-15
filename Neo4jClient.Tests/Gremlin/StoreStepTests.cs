using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class StoreStepTests
    {
        [Test]
        public void StoreVShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).StoreV<object>("foo");
            Assert.AreEqual("foo = [];g.v(p0).store(foo)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void StoreVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).StoreV<object>("foo");
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
        }

        [Test]
        public void StoreEShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).StoreE("foo");
            Assert.AreEqual("foo = [];g.v(p0).store(foo)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void StoreEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).StoreE("foo");
            Assert.IsInstanceOf<GremlinRelationshipEnumerable>(query);
        }

        [Test]
        public void StoreEWithTDataShouldAppendStepAndDeclareVariable()
        {
            var query = new NodeReference(123).StoreE<object>("foo");
            Assert.AreEqual("foo = [];g.v(p0).store(foo)", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
        }

        [Test]
        public void StoreEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).StoreE<object>("foo");
            Assert.IsInstanceOf<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}