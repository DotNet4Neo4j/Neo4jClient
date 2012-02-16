using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryTests
    {
        [Test]
        public void StartAndReturnNodeById()
        {
            // http://docs.neo4j.org/chunked/1.6/query-start.html#start-node-by-id
            // START n=node(1)
            // RETURN n

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)1)
                .Return<object>("n")
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

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .AddStartPoint("b", (NodeReference)2)
                .Query;

            Assert.AreEqual("START a=node({p0}), b=node({p1})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        public void ReturnFirstPart()
        {
            // http://docs.neo4j.org/chunked/1.6/query-limit.html#limit-return-first-part
            // START n=node(3, 4, 5, 1, 2)
            // RETURN n
            // LIMIT 3

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)4, (NodeReference)5, (NodeReference)1, (NodeReference)2)
                .Return<object>("n")
                .Limit(3)
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1}, {p2}, {p3}, {p4})\r\nRETURN n\r\nLIMIT {p5}", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(4, query.QueryParameters["p1"]);
            Assert.AreEqual(5, query.QueryParameters["p2"]);
            Assert.AreEqual(1, query.QueryParameters["p3"]);
            Assert.AreEqual(2, query.QueryParameters["p4"]);
            Assert.AreEqual(3, query.QueryParameters["p5"]);
        }

        [Test]
        public void SkipFirstThree()
        {
            // http://docs.neo4j.org/chunked/1.6/query-skip.html#skip-skip-first-three
            // START n=node(3, 4, 5, 1, 2) 
            // RETURN n 
            // ORDER BY n.name 
            // SKIP 3

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)4, (NodeReference)5, (NodeReference)1, (NodeReference)2)
                .Return<object>("n")
                .OrderBy("n.name")
                .Skip(3)
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1}, {p2}, {p3}, {p4})\r\nRETURN n\r\nORDER BY n.name\r\nSKIP {p5}", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(4, query.QueryParameters["p1"]);
            Assert.AreEqual(5, query.QueryParameters["p2"]);
            Assert.AreEqual(1, query.QueryParameters["p3"]);
            Assert.AreEqual(2, query.QueryParameters["p4"]);
            Assert.AreEqual(3, query.QueryParameters["p5"]);
        }

        [Test]
        public void ReturnMiddleTwo()
        {
            // http://docs.neo4j.org/chunked/1.6/query-skip.html#skip-return-middle-two
            // START n=node(3, 4, 5, 1, 2) 
            // RETURN n 
            // ORDER BY n.name 
            // SKIP 1
            // LIMIT 2

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)4, (NodeReference)5, (NodeReference)1, (NodeReference)2)
                .Return<object>("n")
                .OrderBy("n.name")
                .Skip(1)
                .Limit(2)
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1}, {p2}, {p3}, {p4})\r\nRETURN n\r\nORDER BY n.name\r\nSKIP {p5}\r\nLIMIT {p6}", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(4, query.QueryParameters["p1"]);
            Assert.AreEqual(5, query.QueryParameters["p2"]);
            Assert.AreEqual(1, query.QueryParameters["p3"]);
            Assert.AreEqual(2, query.QueryParameters["p4"]);
            Assert.AreEqual(1, query.QueryParameters["p5"]);
            Assert.AreEqual(2, query.QueryParameters["p6"]);
        }

        [Test]
        public void OrderNodesByNull()
        {
            // http://docs.neo4j.org/chunked/stable/query-order.html#order-by-ordering-null
            // START n=node(3,1,2)
            // RETURN n.length?, n
            // ORDER BY n.length?

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1, (NodeReference)2)
                .Return<object>("n.length?, n")
                .OrderBy("n.length?")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1}, {p2})\r\nRETURN n.length?, n\r\nORDER BY n.length?", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
            Assert.AreEqual(2, query.QueryParameters["p2"]);
        }

        [Test]
        public void OrderNodesByProperty()
        {
            // http://docs.neo4j.org/chunked/stable/query-order.html#order-by-order-nodes-by-property
            // START n=node(3,1,2)
            // RETURN n
            // ORDER BY n.name

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1, (NodeReference)2)
                .Return<object>("n")
                .OrderBy("n.name")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1}, {p2})\r\nRETURN n\r\nORDER BY n.name", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
            Assert.AreEqual(2, query.QueryParameters["p2"]);
        }

        [Test]
        public void OrderNodesByMultipleProperties()
        {
            // http://docs.neo4j.org/chunked/stable/query-order.html#order-by-order-nodes-by-multiple-properties
            // START n=node(3,1,2)
            // RETURN n
            // ORDER BY n.age, n.name

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1, (NodeReference)2)
                .Return<object>("n")
                .OrderBy("n.age", "n.name")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1}, {p2})\r\nRETURN n\r\nORDER BY n.age, n.name", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
            Assert.AreEqual(2, query.QueryParameters["p2"]);
        }

        [Test]
        public void OrderNodesByPropertyDescending()
        {
            // http://docs.neo4j.org/chunked/stable/query-order.html#order-by-order-nodes-in-descending-order
            // START n=node(3,1,2)
            // RETURN n
            // ORDER BY n.name DESC

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1, (NodeReference)2)
                .Return<object>("n")
                .OrderByDescending("n.name")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1}, {p2})\r\nRETURN n\r\nORDER BY n.name DESC", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
            Assert.AreEqual(2, query.QueryParameters["p2"]);
        }

        [Test]
        public void OrderNodesByMultiplePropertiesDescending()
        {
            // http://docs.neo4j.org/chunked/stable/query-order.html#order-by-order-nodes-in-descending-order
            // START n=node(3,1,2)
            // RETURN n
            // ORDER BY n.age, n.name DESC

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1, (NodeReference)2)
                .Return<object>("n")
                .OrderByDescending("n.age", "n.name")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1}, {p2})\r\nRETURN n\r\nORDER BY n.age, n.name DESC", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
            Assert.AreEqual(2, query.QueryParameters["p2"]);
        }

        [Test]
        public void MatchRelatedNodes()
        {
            // http://docs.neo4j.org/chunked/1.6/query-match.html#match-related-nodes
            // START n=node(3)
            // MATCH (n)--(x)
            // RETURN x

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Match("(n)--(x)")
                .Return<object>("x")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nMATCH (n)--(x)\r\nRETURN x", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
        public void ReturnColumnAlias()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Return(a => new ReturnPropertyQueryResult
                {
                    SomethingTotallyDifferent = a.As<FooData>().Age
                })
                .Query;

            Assert.AreEqual("START a=node({p0})\r\nRETURN a.Age AS SomethingTotallyDifferent", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
        }

        [Test]
        public void ReturnUniqueResults()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-unique-results
            // START a=node(1)
            // MATCH (a)-->(b)
            // RETURN distinct b

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Match("(a)-->(b)")
                .ReturnDistinct<object>("b")
                .Query;

            Assert.AreEqual("START a=node({p0})\r\nMATCH (a)-->(b)\r\nRETURN distinct b", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
        }

        [Test]
        public void ReturnUniqueResultsWithExpression()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-unique-results
            // START a=node(1)
            // MATCH (a)-->(b)
            // RETURN distinct b

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Match("(a)-->(b)")
                .ReturnDistinct(b => new FooData
                {
                    Age = b.As<FooData>().Age
                })
                .Query;

            Assert.AreEqual("START a=node({p0})\r\nMATCH (a)-->(b)\r\nRETURN distinct b.Age AS Age", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereBooleanOperations()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Age < 30 && n.Name == "Tobias") || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1})\r\nWHERE (((n.Age < {p2}) AND (n.Name = {p3})) OR (n.Name != {p4}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
            Assert.AreEqual(30, query.QueryParameters["p2"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p3"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p4"]);
        }


        [Test]
        public void WhereFilterOnNodeProperty()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-filter-on-node-property
            // START n=node(3, 1)
            // WHERE n.age < 30
            // RETURN n

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => n.Age < 30 )
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1})\r\nWHERE (n.Age < {p2})\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
            Assert.AreEqual(30, query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereFilterOnRelationshipType()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html
            // START n=node(3)
            // MATCH (n)-[r]->()
            // WHERE type(r) = "HOSTS"
            // RETURN r

            var client = Substitute.For<IGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Match("(n)-[r]->()")
                .Where("type(r) = \"HOSTS\"")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nMATCH (n)-[r]->()\r\nWHERE type(r) = 'HOSTS'\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        //[Test]
        //public void WhereRegularExpression()
        //{
        //    // http://docs.neo4j.org/chunked/1.6/query-where.html#where-filter-on-node-property
        //    // START n=node(3, 1)
        //    // WHERE n.name =~ /Tob.*/
        //    // RETURN n

        //    var client = Substitute.For<IGraphClient>();
        //    var query = new CypherFluentQuery(client)
        //        .Start("n", (NodeReference)3, (NodeReference)1)
        //        .Where<FooNode>(n => n.Name.Regex("/Tob.*/"))
        //        .Return<object>("n")
        //        .Query;

        //    Assert.AreEqual("START n=node({p0}, {p1})\r\nWHERE (n.Age < {p2})\r\nRETURN n".Replace("'", "\""), query.QueryText);
        //    Assert.AreEqual(3, query.QueryParameters["p0"]);
        //    Assert.AreEqual(1, query.QueryParameters["p1"]);
        //    Assert.AreEqual(30, query.QueryParameters["p2"]);
        //}

        public class FooData
        {
            public int Age { get; set; }
            public string Name { get; set; }
        }

        public class ReturnPropertyQueryResult
        {
            public int SomethingTotallyDifferent { get; set; }
        }
    }
}
