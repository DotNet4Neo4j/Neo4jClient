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
                .Start(
                    new CypherStartBitWithNodeIndexLookup("a", "nodes", "name", "A"),
                    new CypherStartBitWithNodeIndexLookup("b", "nodes", "name", "B")
                )
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
                .Start(
                    new CypherStartBitWithNodeIndexLookup("a", "nodes", "name", "A"),
                    new CypherStartBit("b", (NodeReference)2)
                )
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
                .Start(
                    new CypherStartBitWithNodeIndexLookup("a", "nodes", "name", "A"),
                    new CypherStartBit("b", (NodeReference)2),
                    new CypherStartBitWithNodeIndexLookup("c", "nodes", "name", "C")
                )
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
                .Start(
                    new CypherStartBit("a", (NodeReference)1),
                    new CypherStartBitWithNodeIndexLookup("b", "nodes", "name", "B")
                )
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
        public void AllNodesMultipleTimes()
        {
            // http://docs.neo4j.org/chunked/1.8/query-start.html#start-all-nodes
            //START n=node(*), b=node(*)
            //RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(
                    new RawCypherStartBit("n", "node(*)"),
                    new RawCypherStartBit("b", "node(*)")
                )
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node(*), b=node(*)\r\nRETURN n", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test(Description = "Issue 56")]
        public void NodeByAutoIndexLookup()
        {
            // https://bitbucket.org/Readify/neo4jclient/issue/56/cypher-fluent-api-node-auto-index-start
            // http://stackoverflow.com/questions/14882562/cypher-query-return-related-nodes-as-children/14986114
            // start s=node:node_auto_index(StartType='JobTypes')
            // match s-[:starts]->t, t-[:SubTypes]->ts
            // return {Id: t.Id, Name: t.Name, JobSpecialties: ts}

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new CypherStartBitWithNodeIndexLookup("s", "node_auto_index", "StartType", "JobTypes"))
                .Match("s-[:starts]->t", "t-[:SubTypes]->ts")
                .Return((t, ts) => new
                {
                    t.As<JobType>().Id,
                    t.As<JobType>().Name,
                    JobSpecialties = ts.CollectAs<JobSpecialty>()
                })
                .Query;

            Assert.AreEqual(@"START s=node:node_auto_index(StartType = {p0})
MATCH s-[:starts]->t, t-[:SubTypes]->ts
RETURN t.Id? AS Id, t.Name? AS Name, collect(ts) AS JobSpecialties", query.QueryText);
            Assert.AreEqual("JobTypes", query.QueryParameters["p0"]);
        }

        [Test]
        public void MutipleNodesByReference()
        {
            // https://bitbucket.org/Readify/neo4jclient/issue/64/cypher-query-with-multiple-starts
            // START n1=node(1), n2=node(2)
            // MATCH n1-[:KNOWS]->n2
            // RETURN count(*)

            var referenceA = (NodeReference)1;
            var referenceB = (NodeReference)2;

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(
                    new CypherStartBit("n1", referenceA),
                    new CypherStartBit("n2", referenceB)
                )
                .Query;

            Assert.AreEqual("START n1=node({p0}), n2=node({p1})", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        public class JobType
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class JobSpecialty
        {
        }
    }
}
