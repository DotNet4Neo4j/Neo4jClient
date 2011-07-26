using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class BackTests
    {
        [Test]
        public void BackVWithCountShouldAppendStep()
        {
            var node = new NodeReference(123);
            var queryText = node.BackV<object>(2).QueryText;
            Assert.AreEqual("g.v(123).back(2)", queryText);
        }

        [Test]
        public void BackVWithCountShouldReturnTypedNodeEnumerable()
        {
            var node = new NodeReference(123);
            var query = node.BackV<object>(2);
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
        }

        [Test]
        public void BackEWithCountShouldAppendStep()
        {
            var node = new NodeReference(123);
            var queryText = node.BackE(2).QueryText;
            Assert.AreEqual("g.v(123).back(2)", queryText);
        }

        [Test]
        public void BackEWithCountShouldReturnRelationshipEnumerable()
        {
            var node = new NodeReference(123);
            var query = node.BackE(2);
            Assert.IsInstanceOf<GremlinRelationshipEnumerable>(query);
        }
    }
}