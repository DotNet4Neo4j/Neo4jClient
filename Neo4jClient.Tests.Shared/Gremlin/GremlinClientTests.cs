using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class GremlinClientTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void VShouldEumerateAllVertexes()
        {
            var client = Substitute.For<IGraphClient>();
            var gremlinClient = new GremlinClient(client);
            var query = gremlinClient.V;
            Assert.Equal("g.V", query.QueryText);
        }

        [Fact]
        public void VShouldCombineWithGremlinCount()
        {
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteScalarGremlin("g.V.count()", Arg.Is((IDictionary<string, object> d) => d.Count == 0))
                .Returns("123");

            var gremlinClient = new GremlinClient(client);
            client.Gremlin.Returns(gremlinClient);

            var result = gremlinClient.V.GremlinCount();
            Assert.Equal(123, result);
        }

        [Fact]
        public void EShouldEumerateAllEdges()
        {
            var client = Substitute.For<IGraphClient>();
            var gremlinClient = new GremlinClient(client);
            var query = gremlinClient.E;
            Assert.Equal("g.E", query.QueryText);
        }

        [Fact]
        public void EShouldCombineWithGremlinCount()
        {
            var client = Substitute.For<IGraphClient>();
            client
                .ExecuteScalarGremlin("g.E.count()", Arg.Is((IDictionary<string, object> d) => d.Count == 0))
                .Returns("123");

            var gremlinClient = new GremlinClient(client);
            client.Gremlin.Returns(gremlinClient);

            var result = gremlinClient.E.GremlinCount();
            Assert.Equal(123, result);
        }
    }
}
