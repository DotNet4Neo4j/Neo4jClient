using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class GremlinClientTests
    {
        [Test]
        public void VShouldEumerateAllVertexes()
        {
            var client = Substitute.For<IGraphClient>();
            var gremlinClient = new GremlinClient(client);
            var query = gremlinClient.V;
            Assert.AreEqual("g.V", query.QueryText);
        }

        [Test]
        public void VShouldCombineWithGremlinCount()
        {
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteScalarGremlin("g.V.count()", Arg.Is((IDictionary<string, object> d) => d.Count == 0))
                .Returns("123");

            var gremlinClient = new GremlinClient(client);
            client.Gremlin.Returns(gremlinClient);

            var result = gremlinClient.V.GremlinCount();
            Assert.AreEqual(123, result);
        }

        [Test]
        public void EShouldEumerateAllEdges()
        {
            var client = Substitute.For<IGraphClient>();
            var gremlinClient = new GremlinClient(client);
            var query = gremlinClient.E;
            Assert.AreEqual("g.E", query.QueryText);
        }

        [Test]
        public void EShouldCombineWithGremlinCount()
        {
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteScalarGremlin("g.E.count()", Arg.Is((IDictionary<string, object> d) => d.Count == 0))
                .Returns("123");

            var gremlinClient = new GremlinClient(client);
            client.Gremlin.Returns(gremlinClient);

            var result = gremlinClient.E.GremlinCount();
            Assert.AreEqual(123, result);
        }
    }
}
