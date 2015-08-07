using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class IteratorTests
    {
        [Test]
        public void GremlinSkipVShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutV<object>().GremlinSkip<object>(5);
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
            Assert.AreEqual("g.v(p0).outV.drop(p1)._()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
        }

        [Test]
        public void GremlinSkipEShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutE().GremlinSkip(5);
            Assert.IsInstanceOf<GremlinRelationshipEnumerable>(query);
            Assert.AreEqual("g.v(p0).outE.drop(p1)._()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
        }

        [Test]
        public void GremlinSkipEWithTDataShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutE<object>().GremlinSkip<object>(5);
            Assert.IsInstanceOf<GremlinRelationshipEnumerable<object>>(query);
            Assert.AreEqual("g.v(p0).outE.drop(p1)._()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
        }

        [Test]
        public void GremlinTakeVShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutV<object>().GremlinTake<object>(5);
            Assert.IsInstanceOf<GremlinNodeEnumerable<object>>(query);
            Assert.AreEqual("g.v(p0).outV.take(p1)._()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
        }

        [Test]
        public void GremlinTakeEShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutE().GremlinTake(5);
            Assert.IsInstanceOf<GremlinRelationshipEnumerable>(query);
            Assert.AreEqual("g.v(p0).outE.take(p1)._()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
        }

        [Test]
        public void GremlinTakeEWithTDataShouldAppendStep()
        {
            var node = new NodeReference(123);
            var query = node.OutE<object>().GremlinTake<object>(5);
            Assert.IsInstanceOf<GremlinRelationshipEnumerable<object>>(query);
            Assert.AreEqual("g.v(p0).outE.take(p1)._()", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
        }
    }
}