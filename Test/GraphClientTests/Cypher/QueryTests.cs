using System;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class QueryTests
    {
        readonly Uri fakeEndpoint = new Uri("http://test.example.com/foo");

        [Test]
        public void StartAndReturnNodeById()
        {
            // http://docs.neo4j.org/chunked/1.6/query-start.html#start-node-by-id
            // START n=node(1)
            // RETURN n

            var client = new GraphClient(fakeEndpoint);
            var query = client
                .Cypher
                .Start("n", (NodeReference) 1)
                .Return("n")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nRETURN n", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
        }

        [Test]
        public void MultipleStartPoints()
        {
            // http://docs.neo4j.org/chunked/1.6/query-start.html#start-multiple-start-points
            // START a=node(1), b=node(2)
            // RETURN a,b

            var client = new GraphClient(fakeEndpoint);
            var query = client
                .Cypher
                .Start("a", (NodeReference)1)
                .AddStartPoint("b", (NodeReference)2)
                .Return("a", "b")
                .Query;

            Assert.AreEqual("START a=node({p0}), b=node({p1})\r\nRETURN a, b", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }
    }
}
