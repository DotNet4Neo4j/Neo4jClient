using System;
using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    public class CypherFluentQueryStartTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        [Obsolete]
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

            Assert.Equal("START n=node:`nodes`(name = {p0})\r\nRETURN n", query.QueryText);
            Assert.Equal("A", query.QueryParameters["p0"]);
        }

        [Fact]
        [Obsolete]
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

            Assert.Equal("START a=node:`nodes`(name = {p0}), b=node:`nodes`(name = {p1})\r\nRETURN a", query.QueryText);
            Assert.Equal("A", query.QueryParameters["p0"]);
            Assert.Equal("B", query.QueryParameters["p1"]);
        }

        [Fact]
        [Obsolete]
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
            
            Assert.Equal("START a=node:`nodes`(name = {p0}), b=node({p1})\r\nRETURN a", query.QueryText);
            Assert.Equal("A", query.QueryParameters["p0"]);
            Assert.Equal(2L, query.QueryParameters["p1"]);
        }

        [Fact]
        [Obsolete]
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
            
            Assert.Equal("START a=node:`nodes`(name = {p0}), b=node({p1}), c=node:`nodes`(name = {p2})\r\nRETURN a", query.QueryText);
            Assert.Equal("A", query.QueryParameters["p0"]);
            Assert.Equal(2L, query.QueryParameters["p1"]);
            Assert.Equal("C", query.QueryParameters["p2"]);
        }

        [Fact]
        [Obsolete]
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
            
            Assert.Equal("START a=node({p0}), b=node:`nodes`(name = {p1})\r\nRETURN a", query.QueryText);
            Assert.Equal(1L, query.QueryParameters["p0"]);
            Assert.Equal("B", query.QueryParameters["p1"]);
        }

        [Fact]
        [Obsolete]
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

            Assert.Equal("START n=node:`nodes`({p0})\r\nRETURN n", query.QueryText);
            Assert.Equal("*.*", query.QueryParameters["p0"]);
        }

        [Fact]
        public void AllNodes()
        {
            // http://docs.neo4j.org/chunked/1.8/query-start.html#start-all-nodes
            //START n=node(*)
            //RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { n = All.Nodes })
                .Return<object>("n")
                .Query;

            Assert.Equal("START n=node(*)\r\nRETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void AllNodesMultipleTimes()
        {
            // http://docs.neo4j.org/chunked/1.8/query-start.html#start-all-nodes
            //START n=node(*), b=node(*)
            //RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new
                {
                    n = All.Nodes,
                    b = All.Nodes
                })
                .Return<object>("n")
                .Query;

            Assert.Equal("START n=node(*), b=node(*)\r\nRETURN n", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/56/cypher-fluent-api-node-auto-index-start")]
        public void NodeByAutoIndexLookup()
        {
            // http://stackoverflow.com/questions/14882562/cypher-query-return-related-nodes-as-children/14986114
            // start s=node:node_auto_index(StartType='JobTypes')
            // match s-[:starts]->t, t-[:SubTypes]->ts
            // return {Id: t.Id, Name: t.Name, JobSpecialties: ts}

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { s = Node.ByIndexLookup("node_auto_index", "StartType", "JobTypes") })
                .Match("s-[:starts]->t", "t-[:SubTypes]->ts")
                .Return((t, ts) => new
                {
                    t.As<JobType>().Id,
                    t.As<JobType>().Name,
                    JobSpecialties = ts.CollectAs<JobSpecialty>()
                })
                .Query;

            Assert.Equal(string.Format("START s=node:`node_auto_index`(StartType = {{p0}}){0}MATCH s-[:starts]->t, t-[:SubTypes]->ts{0}RETURN t.Id AS Id, t.Name AS Name, collect(ts) AS JobSpecialties", Environment.NewLine), query.QueryText);
            Assert.Equal("JobTypes", query.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/64/cypher-query-with-multiple-starts")]
        public void MutipleNodesByReference()
        {
            // START n1=node(1), n2=node(2)
            // MATCH n1-[:KNOWS]->n2
            // RETURN count(*)

            var referenceA = (NodeReference)1;
            var referenceB = (NodeReference)2;

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new {
                    n1 = referenceA,
                    n2 = referenceB
                })
                .Query;

            Assert.Equal("START n1=node({p0}), n2=node({p1})", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(1L, query.QueryParameters["p0"]);
            Assert.Equal(2L, query.QueryParameters["p1"]);
        }

        [Fact]
        [Obsolete]
        public void MutipleNodesByReferenceObsolete()
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

            Assert.Equal("START n1=node({p0}), n2=node({p1})", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(1L, query.QueryParameters["p0"]);
            Assert.Equal(2L, query.QueryParameters["p1"]);
        }

        [Fact]
        public void SingleNodeByStaticReferenceInAnonymousType()
        {
            // START n1=node(1)

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new
                {
                    n1 = (NodeReference)1
                })
                .Query;

            Assert.Equal("START n1=node({p0})", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(1L, query.QueryParameters["p0"]);
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
