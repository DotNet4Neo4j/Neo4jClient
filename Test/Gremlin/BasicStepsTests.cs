using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class BasicStepsTests
    {
        [Test]
        public void OutVShouldAppendStepToNodeReference()
        {
            var node = new NodeReference(123);
            var queryText = node.OutV<object>().QueryText;
            Assert.AreEqual("g.v(123).outV", queryText);
        }

        [Test]
        public void OutVShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.OutV<object>().OutV<object>().QueryText;
            Assert.AreEqual("g.v(123).outV.outV", queryText);
        }

        [Test]
        public void OutVShouldReturnTypedGremlinEnumerable()
        {
            var node = new NodeReference(123);
            var query = node.OutV<object>();
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
        }

        [Test]
        public void OutVShouldCombineWithInE()
        {
            var node = new NodeReference(123);
            var queryText = node.InE().OutV<object>().QueryText;
            Assert.AreEqual("g.v(123).inE.outV", queryText);
        }

        [Test]
        public void InVShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.InV<object>().QueryText;
            Assert.AreEqual("g.v(123).inV", queryText);
        }

        [Test]
        public void InVShouldReturnTypedGremlinEnumerable()
        {
            var node = new NodeReference(123);
            var query = node.InV<object>();
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
        }

        [Test]
        public void InVShouldCombineWithOutE()
        {
            var node = new NodeReference(123);
            var queryText = node.OutE().InV<object>().QueryText;
            Assert.AreEqual("g.v(123).outE.inV", queryText);
        }

        [Test]
        public void OutEShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.OutE().QueryText;
            Assert.AreEqual("g.v(123).outE", queryText);
        }

        [Test]
        public void InEShouldAppendStepToGremlinQuery()
        {
            var node = new NodeReference(123);
            var queryText = node.InE().QueryText;
            Assert.AreEqual("g.v(123).inE", queryText);
        }
    }
}