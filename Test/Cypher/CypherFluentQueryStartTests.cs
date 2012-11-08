using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    public class CypherFluentQueryStartTests
    {
        [Test]
        public void NodeByIndexLookup()
        {
            // http://docs.neo4j.org/chunked/1.8.M07/query-start.html#start-node-by-index-lookup
            //START n=node:nodes(name = "A")
            //RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .StartWithNodeIndexLookup("n", "nodes", "name", "A")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node:nodes(name = {p0})\r\nRETURN n", query.QueryText);
            Assert.AreEqual("A", query.QueryParameters["p0"]);
        }

        [Test]
        public void NodeByIndexLookupMultipleIndexedStartPoints() 
        {
            // http://docs.neo4j.org/chunked/1.8.M07/query-start.html#start-node-by-index-lookup
            //START a=node:nodes(name = "A"), b=node:nodes(name = "B")
            //RETURN a

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .StartWithNodeIndexLookup("a", "nodes", "name", "A")
                .AddStartPointWithNodeIndexLookup("b", "nodes", "name", "B")
                .Return<object>("a")
                .Query;

            Assert.AreEqual("START a=node:nodes(name = {p0}), b=node:nodes(name = {p1})\r\nRETURN a", query.QueryText);
            Assert.AreEqual("A", query.QueryParameters["p0"]);
            Assert.AreEqual("B", query.QueryParameters["p1"]);
        }

        [Test]
        public void NodeByIndexLookupWithAdditionalStartPoint() 
        {
            // http://docs.neo4j.org/chunked/1.8.M07/query-start.html#start-node-by-index-lookup
            //START a=node:nodes(name = "A"), b=node(2)
            //RETURN a
            
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .StartWithNodeIndexLookup("a", "nodes", "name", "A")
                .AddStartPoint("b", (NodeReference)2)
                .Return<object>("a")
                .Query;
            
            Assert.AreEqual("START a=node:nodes(name = {p0}), b=node({p1})\r\nRETURN a", query.QueryText);
            Assert.AreEqual("A", query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        public void NodeByIndexLookupWithAdditionalStartPointAndExtraIndexedStartPoint() 
        {
            // http://docs.neo4j.org/chunked/1.8.M07/query-start.html#start-node-by-index-lookup
            //START a=node:nodes(name = "A"), b=node(2), c=node:nodes(name = "C")
            //RETURN a
            
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .StartWithNodeIndexLookup("a", "nodes", "name", "A")
                .AddStartPoint("b", (NodeReference)2)
                .AddStartPointWithNodeIndexLookup("c", "nodes", "name", "C")
                .Return<object>("a")
                .Query;
            
            Assert.AreEqual("START a=node:nodes(name = {p0}), b=node({p1}), c=node:nodes(name = {p2})\r\nRETURN a", query.QueryText);
            Assert.AreEqual("A", query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
            Assert.AreEqual("C", query.QueryParameters["p2"]);
        }

        [Test]
        public void StartThenNodeByIndexLookup() 
        {
            // http://docs.neo4j.org/chunked/1.8.M07/query-start.html#start-node-by-index-lookup
            //START a=nodes(2), b=node:nodes(name = "B")
            //RETURN a
            
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .AddStartPointWithNodeIndexLookup("b", "nodes", "name", "B")
                .Return<object>("a")
                .Query;
            
            Assert.AreEqual("START a=node({p0}), b=node:nodes(name = {p1})\r\nRETURN a", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual("B", query.QueryParameters["p1"]);
        }

        [Test]
        public void NodeByIndexLookupWithSingleParameter()
        {
            // http://docs.neo4j.org/chunked/1.8.M07/query-start.html#start-node-by-index-lookup
            //START n=node:nodes("*:*")
            //RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .StartWithNodeIndexLookup("n", "nodes", "*.*")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node:nodes({p0})\r\nRETURN n", query.QueryText);
            Assert.AreEqual("*.*", query.QueryParameters["p0"]);
        }

        [Test]
        public void AllNodes()
        {
            // http://docs.neo4j.org/chunked/1.8/query-start.html#start-all-nodes
            //START n=node(*)
            //RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", "node(*)")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node(*)\r\nRETURN n", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void AddAllNodes()
        {
            // http://docs.neo4j.org/chunked/1.8/query-start.html#start-all-nodes
            //START n=node(*)
            //RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", "node(*)")
                .AddStartPoint("b", "node(*)")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node(*), b=node(*)\r\nRETURN n", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }
    }
}
