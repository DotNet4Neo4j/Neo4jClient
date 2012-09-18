using System;
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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

            var client = Substitute.For<IRawGraphClient>();
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
        public void ReturnPropertiesIntoAnonymousType()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Match("(a)-->(b)")
                .Return(b => new
                {
                    SomeAge = b.As<FooData>().Age,
                    SomeName = b.As<FooData>().Name
                })
                .Query;

            const string expected = @"
START a=node({p0})
MATCH (a)-->(b)
RETURN b.Age AS SomeAge, b.Name? AS SomeName";

            Assert.AreEqual(expected.TrimStart(new[] { '\r', '\n' }), query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
        }

        [Test]
        public void ReturnPropertiesIntoAnonymousTypeWithAutoNames()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Match("(a)-->(b)")
                .Return(b => new
                {
                    b.As<FooData>().Age,
                    b.As<FooData>().Name
                })
                .Query;

            const string expected = @"
START a=node({p0})
MATCH (a)-->(b)
RETURN b.Age AS Age, b.Name? AS Name";

            Assert.AreEqual(expected.TrimStart(new[] { '\r', '\n' }), query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
        }

        [Test]
        public void ReturnPropertiesFromMultipleNodesIntoAnonymousTypeWithAutoNames()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Match("(a)-->(b)-->(c)")
                .Return((b, c) => new
                {
                    b.As<FooData>().Age,
                    c.As<FooData>().Name
                })
                .Query;

            const string expected = @"
START a=node({p0})
MATCH (a)-->(b)-->(c)
RETURN b.Age AS Age, c.Name? AS Name";

            Assert.AreEqual(expected.TrimStart(new[] { '\r', '\n' }), query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
        }

        [Test]
        public void ReturnNodeDataIntoAnonymousType()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Match("(a)-->(b)")
                .Return((b, c) => new
                {
                    NodeB = b.As<FooData>(),
                })
                .Query;

            const string expected = @"
START a=node({p0})
MATCH (a)-->(b)
RETURN b AS NodeB";

            Assert.AreEqual(expected.TrimStart(new[] { '\r', '\n' }), query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
        }

        [Test]
        public void ReturnEntireNodeDataAndReferenceIntoAnonymousType()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Match("(a)-->(b)")
                .Return((b, c) => new
                {
                    NodeB = b.Node<FooData>(),
                })
                .Query;

            const string expected = @"
START a=node({p0})
MATCH (a)-->(b)
RETURN b AS NodeB";

            Assert.AreEqual(expected.TrimStart(new[] { '\r', '\n' }), query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
        }

        [Test]
        public void ReturnEntireNodeDataAndReferenceIntoProjectionType()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .Match("(a)-->(b)")
                .Return((b, c) => new ReturnEntireNodeDataAndReferenceIntoProjectionTypeResult
                {
                    NodeB = b.Node<FooData>(),
                })
                .Query;

            const string expected = @"
START a=node({p0})
MATCH (a)-->(b)
RETURN b AS NodeB";

            Assert.AreEqual(expected.TrimStart(new[] { '\r', '\n' }), query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
        }

        public class ReturnEntireNodeDataAndReferenceIntoProjectionTypeResult
        {
            public Node<FooData> NodeB { get; set; }
        }

        [Test]
        public void WhereBooleanOperationWithVariable()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            const string name = "Tobias";

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Age < 30 && n.Name == name) || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p3}, {p4})\r\nWHERE (((n.Age < {p0}) AND (n.Name? = {p1})) OR (n.Name? != {p2}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p3"]);
            Assert.AreEqual(1, query.QueryParameters["p4"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p1"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithLong()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            const string name = "Tobias";

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.AgeLong < 30 && n.Name == name) || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p3}, {p4})\r\nWHERE (((n.AgeLong < {p0}) AND (n.Name? = {p1})) OR (n.Name? != {p2}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p3"]);
            Assert.AreEqual(1, query.QueryParameters["p4"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p1"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithNullableLong()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            const string name = "Tobias";

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.AgeLongNullable < 30 && n.Name == name) || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p3}, {p4})\r\nWHERE (((n.AgeLongNullable? < {p0}) AND (n.Name? = {p1})) OR (n.Name? != {p2}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p3"]);
            Assert.AreEqual(1, query.QueryParameters["p4"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p1"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithStringPropertyOnRightSide()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            var foo = new FooData
                {
                    Name = "Tobias"
                };

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Age < 30 && n.Name == foo.Name) || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p3}, {p4})\r\nWHERE (((n.Age < {p0}) AND (n.Name? = {p1})) OR (n.Name? != {p2}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p3"]);
            Assert.AreEqual(1, query.QueryParameters["p4"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p1"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithIntPropertyOnRightSide()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            var foo = new FooData
            {
                Name = "Tobias",
                Id = 777
            };

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Age < 30 && n.Id == foo.Id) || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p3}, {p4})\r\nWHERE (((n.Age < {p0}) AND (n.Id? = {p1})) OR (n.Name? != {p2}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p3"]);
            Assert.AreEqual(1, query.QueryParameters["p4"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual(777, query.QueryParameters["p1"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithIntConstantOnRightSide()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            int theId = 777;

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Id == theId))
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p1}, {p2})\r\nWHERE (n.Id? = {p0})\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(777, query.QueryParameters["p0"]);
            Assert.AreEqual(3, query.QueryParameters["p1"]);
            Assert.AreEqual(1, query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithLongConstantOnRightSide()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            long theId = 777;

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Id == theId))
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p1}, {p2})\r\nWHERE (n.Id? = {p0})\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(777, query.QueryParameters["p0"]);
            Assert.AreEqual(3, query.QueryParameters["p1"]);
            Assert.AreEqual(1, query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithLongNullableConstantOnRightSide()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            long? theId = 777;

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Id == theId))
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p1}, {p2})\r\nWHERE (n.Id? = {p0})\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(777, query.QueryParameters["p0"]);
            Assert.AreEqual(3, query.QueryParameters["p1"]);
            Assert.AreEqual(1, query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithObjectProperty()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            var fooData = new FooData {Name = "Tobias"};

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Age < 30 && n.Name == fooData.Name) || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p3}, {p4})\r\nWHERE (((n.Age < {p0}) AND (n.Name? = {p1})) OR (n.Name? != {p2}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p3"]);
            Assert.AreEqual(1, query.QueryParameters["p4"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p1"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperationWithObjectNullableProperty()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            var fooData = new FooData { Id = 777 };

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Age < 30 && n.Id == fooData.Id) || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p3}, {p4})\r\nWHERE (((n.Age < {p0}) AND (n.Id? = {p1})) OR (n.Name? != {p2}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p3"]);
            Assert.AreEqual(1, query.QueryParameters["p4"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual(777, query.QueryParameters["p1"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereBooleanOperations()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-boolean-operations
            // START n=node(3, 1)
            // WHERE (n.age < 30 and n.name = "Tobias") or not(n.name = "Tobias")
            // RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => (n.Age < 30 && n.Name == "Tobias") || n.Name != "Tobias")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p3}, {p4})\r\nWHERE (((n.Age < {p0}) AND (n.Name? = {p1})) OR (n.Name? != {p2}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p3"]);
            Assert.AreEqual(1, query.QueryParameters["p4"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p1"]);
            Assert.AreEqual("Tobias", query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereFilterOnNodeProperty()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-filter-on-node-property
            // START n=node(3, 1)
            // WHERE n.age < 30
            // RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => n.Age < 30 )
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p1}, {p2})\r\nWHERE (n.Age < {p0})\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p1"]);
            Assert.AreEqual(1, query.QueryParameters["p2"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereFilterPropertyExists()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-property-exists
            // START n=node(3, 1)
            // WHERE n.Belt
            // RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => n.Name != null)
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1})\r\nWHERE (n.Name)\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
        }

        [Test]
        public void WhereFilterCompareIfPropertyExists()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-compare-if-property-exists
            // START n=node(3, 1)
            // WHERE n.Belt? = 'white'
            // RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => n.Id < 30)
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p1}, {p2})\r\nWHERE (n.Id? < {p0})\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p1"]);
            Assert.AreEqual(1, query.QueryParameters["p2"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereFilterOnNullValues()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-filter-on-null-values
            // START a=node(1), b=node(3, 2)
            // MATCH a<-[r?]-b
            // WHERE r is null
            // RETURN b

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .AddStartPoint("b", (NodeReference)3, (NodeReference)2)
                .Match("a<-[r?]-b")
                .Where<FooData>(r => r.Name == null && r.Id == 100)
                .Return<object>("b")
                .Query;

            Assert.AreEqual("START a=node({p1}), b=node({p2}, {p3})\r\nMATCH a<-[r?]-b\r\nWHERE ((r.Name is null) AND (r.Id? = {p0}))\r\nRETURN b".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
            Assert.AreEqual(3, query.QueryParameters["p2"]);
            Assert.AreEqual(2, query.QueryParameters["p3"]);
            Assert.AreEqual(100, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereFilterOnMultipleNodesProperties()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData, BarData>((n1, n2) => n1.Age < 30 && n2.Key == 11)
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p2}, {p3})\r\nWHERE ((n1.Age < {p0}) AND (n2.Key = {p1}))\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p2"]);
            Assert.AreEqual(1, query.QueryParameters["p3"]);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
            Assert.AreEqual(11, query.QueryParameters["p1"]);
        }

        [Test]
        public void WhereFilterOnRelationshipType()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html
            // START n=node(3)
            // MATCH (n)-[r]->()
            // WHERE type(r) = "HOSTS"
            // RETURN r

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Match("(n)-[r]->()")
                .Where("type(r) = \"HOSTS\"")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nMATCH (n)-[r]->()\r\nWHERE (type(r) = 'HOSTS')\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereWithAnd()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => n.Name != null)
                .And()
                .Where("type(r) = \"HOSTS\"")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1})\r\nWHERE (n.Name) AND (type(r) = 'HOSTS')\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
        }

        [Test]
        public void WhereWithOr()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => n.Name != null)
                .Or()
                .Where("type(r) = \"HOSTS\"")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1})\r\nWHERE (n.Name) OR (type(r) = 'HOSTS')\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
        }

        [Test]
        public void WhereWithOrAnd()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where<FooData>(n => n.Name != null)
                .Or()
                .Where("type(r) = \"HOSTS\"")
                .And()
                .Where<FooData>(n => n.Id == 10)
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p1}, {p2})\r\nWHERE (n.Name) OR (type(r) = 'HOSTS') AND (n.Id? = {p0})\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p1"]);
            Assert.AreEqual(1, query.QueryParameters["p2"]);
            Assert.AreEqual(10, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereFilterOnRelationships()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-filter-on-relationships
            // START a=node(1), b=node(3, 2)
            // WHERE a<--b
            // RETURN b

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)1)
                .AddStartPoint("b", (NodeReference)3,(NodeReference)2)
                .Where("a<--b")
                .Return<object>("b")
                .Query;

            Assert.AreEqual("START a=node({p0}), b=node({p1}, {p2})\r\nWHERE (a<--b)\r\nRETURN b".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(3, query.QueryParameters["p1"]);
            Assert.AreEqual(2, query.QueryParameters["p2"]);
        }

        [Test]
        public void WhereFilterRegularExpressions()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-regular-expressions
            // START n=node(3, 1)
            // WHERE n.name =~ /Tob.*/
            // RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3, (NodeReference)1)
                .Where("n.Name =~ /Tob.*/")
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0}, {p1})\r\nWHERE (n.Name =~ /Tob.*/)\r\nRETURN n".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(1, query.QueryParameters["p1"]);
        }

        [Test]
        public void CreateRelationshipBetweenTwoNodes()
        {
            //http://docs.neo4j.org/chunked/1.8.M06/query-create.html#create-create-a-relationship-between-two-nodes
            // START a=node(1), b=node(2)
            // CREATE a-[r:REL]->b
            // RETURN r

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .AddStartPoint("a", (NodeReference)1)
                .AddStartPoint("b", (NodeReference)2)
                .Create("a-[r:REL]->b")
                .Return<object>("r")
                .Query;

            Assert.AreEqual("START a=node({p0}), b=node({p1})\r\nCREATE a-[r:REL]->b\r\nRETURN r", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        public void CreateRelationshipAndSetProperties()
        {
            //http://docs.neo4j.org/chunked/1.8.M06/query-create.html#create-create-a-relationship-and-set-properties
            //START a=node(1), b=node(2)
            //CREATE a-[r:REL {name : a.name + '<->' + b.name }]->b
            //RETURN r

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .AddStartPoint("a", (NodeReference)1)
                .AddStartPoint("b", (NodeReference)2)
                .Create("a-[r:REL {name : a.name + '<->' + b.name }]->b")
                .Return<object>("r")
                .Query;

            Assert.AreEqual("START a=node({p0}), b=node({p1})\r\nCREATE a-[r:REL {name : a.name + '<->' + b.name }]->b\r\nRETURN r", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        public void ComplexMatching()
        {
            // http://docs.neo4j.org/chunked/1.8.M03/query-match.html#match-complex-matching
            // START a=node(3)
            // MATCH (a)-[:KNOWS]->(b)-[:KNOWS]->(c), (a)-[:BLOCKS]-(d)-[:KNOWS]-(c)
            // RETURN a,b,c,d

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("a", (NodeReference)3)
                .Match(
                    "(a)-[:KNOWS]->(b)-[:KNOWS]->(c)",
                    "(a)-[:BLOCKS]-(d)-[:KNOWS]-(c)")
                .Query;

            Assert.AreEqual("START a=node({p0})\r\nMATCH (a)-[:KNOWS]->(b)-[:KNOWS]->(c), (a)-[:BLOCKS]-(d)-[:KNOWS]-(c)", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        public class FooData
        {
            public int Age { get; set; }
            public int? Id { get; set; }
            public long AgeLong { get; set; }
            public long? AgeLongNullable { get; set; }
            public string Name { get; set; }
        }

        public class BarData
        {
            public int Key { get; set; }
            public string Value { get; set; }
        }

        public class ReturnPropertyQueryResult
        {
            public int SomethingTotallyDifferent { get; set; }
        }
    }
}
