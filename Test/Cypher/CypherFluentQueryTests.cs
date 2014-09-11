using System;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;
using Neo4jClient.Test.GraphClientTests;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryTests
    {
        [Test]
        public void ExecutesQuery()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            CypherQuery executedQuery = null;
            client
                .When(c => c.ExecuteCypher(Arg.Any<CypherQuery>()))
                .Do(ci => { executedQuery = ci.Arg<CypherQuery>(); });

            // Act
            new CypherFluentQuery(client)
                .Start("n", (NodeReference) 5)
                .Delete("n")
                .ExecuteWithoutResults();

            // Assert
            Assert.IsNotNull(executedQuery, "Query was not executed against graph client");
            Assert.AreEqual("START n=node({p0})\r\nDELETE n", executedQuery.QueryText);
            Assert.AreEqual(5, executedQuery.QueryParameters["p0"]);
        }

        [Test]
        public void ExecutesQueryAsync()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            CypherQuery executedQuery = null;
            client
                .When(c => c.ExecuteCypherAsync(Arg.Any<CypherQuery>()))
                .Do(ci => { executedQuery = ci.Arg<CypherQuery>(); });

            // Act
            var task = new CypherFluentQuery(client)
                .Start("n", (NodeReference) 5)
                .Delete("n")
                .ExecuteWithoutResultsAsync();
            task.Wait();

            // Assert
            Assert.IsNotNull(executedQuery, "Query was not executed against graph client");
            Assert.AreEqual("START n=node({p0})\r\nDELETE n", executedQuery.QueryText);
            Assert.AreEqual(5, executedQuery.QueryParameters["p0"]);
        }

        [Test]
        public void ShouldBuildQueriesAsImmutableStepsInsteadOfCorruptingPreviousOnes()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)1)
                .Return<object>("n");

            var query1 = query.Query;
            query = query.OrderBy("n.Foo");
            var query2 = query.Query;

            Assert.AreEqual("START n=node({p0})\r\nRETURN n", query1.QueryText);
            Assert.AreEqual(1, query1.QueryParameters["p0"]);

            Assert.AreEqual("START n=node({p0})\r\nRETURN n\r\nORDER BY n.Foo", query2.QueryText);
            Assert.AreEqual(1, query2.QueryParameters["p0"]);
        }


        [Test]
        public void AddingStartBitsToDifferentQueriesShouldntCorruptEachOther()
        {
            var client = Substitute.For<IRawGraphClient>();
            var cypher = new CypherFluentQuery(client);

            var query1 = cypher
                .Start("a", (NodeReference)1)
                .Query;

            var query2 = cypher
                .Start("b", (NodeReference)2)
                .Query;

            Assert.AreEqual("START a=node({p0})", query1.QueryText);
            Assert.AreEqual(1, query1.QueryParameters.Count);
            Assert.AreEqual(1, query1.QueryParameters["p0"]);

            Assert.AreEqual("START b=node({p0})", query2.QueryText);
            Assert.AreEqual(1, query2.QueryParameters.Count);
            Assert.AreEqual(2, query2.QueryParameters["p0"]);
        }

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
                .Start(new
                {
                    a = (NodeReference)1,
                    b = (NodeReference)2
                })
                .Query;

            Assert.AreEqual("START a=node({p0}), b=node({p1})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        [Obsolete]
        public void MultipleStartPointsObsolete()
        {
            // http://docs.neo4j.org/chunked/1.6/query-start.html#start-multiple-start-points
            // START a=node(1), b=node(2)
            // RETURN a,b

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(
                    new CypherStartBit("a", (NodeReference)1),
                    new CypherStartBit("b", (NodeReference)2)
                )
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
                .Return<object>("n")
                .Limit(3)
                .Query;

            Assert.AreEqual("RETURN n\r\nLIMIT {p0}", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
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
                .Return<object>("n")
                .OrderBy("n.name")
                .Skip(3)
                .Query;

            Assert.AreEqual("RETURN n\r\nORDER BY n.name\r\nSKIP {p0}", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
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
                .Return<object>("n")
                .OrderBy("n.name")
                .Skip(1)
                .Limit(2)
                .Query;

            Assert.AreEqual("RETURN n\r\nORDER BY n.name\r\nSKIP {p0}\r\nLIMIT {p1}", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        public void OrderNodesByNull()
        {
            // http://docs.neo4j.org/chunked/stable/query-order.html#order-by-ordering-null
            // RETURN n
            // ORDER BY n.length?

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<object>("n")
                .OrderBy("n.length?")
                .Query;

            Assert.AreEqual("RETURN n\r\nORDER BY n.length?", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
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
                .Return<object>("n")
                .OrderBy("n.name")
                .Query;

            Assert.AreEqual("RETURN n\r\nORDER BY n.name", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
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
                .Return<object>("n")
                .OrderBy("n.age", "n.name")
                .Query;

            Assert.AreEqual("RETURN n\r\nORDER BY n.age, n.name", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
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
                .Return<object>("n")
                .OrderByDescending("n.name")
                .Query;

            Assert.AreEqual("RETURN n\r\nORDER BY n.name DESC", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
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
                .Return<object>("n")
                .OrderByDescending("n.age", "n.name")
                .Query;

            Assert.AreEqual("RETURN n\r\nORDER BY n.age, n.name DESC", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
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
RETURN b.Age AS SomeAge, b.Name AS SomeName";

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
RETURN b.Age AS Age, b.Name AS Name";

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
RETURN b.Age AS Age, c.Name AS Name";

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
                .Where<FooData>(n => (n.Age < 30 && n.Name == name) || n.Name != "Tobias")
                .Query;

            Assert.AreEqual("WHERE (((n.Age < {p0}) AND (n.Name = {p1})) OR (n.Name <> {p2}))", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
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
                .Where<FooData>(n => (n.AgeLong < 30 && n.Name == name) || n.Name != "Tobias")
                .Query;

            Assert.AreEqual("WHERE (((n.AgeLong < {p0}) AND (n.Name = {p1})) OR (n.Name <> {p2}))".Replace("'", "\""), query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
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
                .Where<FooData>(n => (n.AgeLongNullable < 30 && n.Name == name) || n.Name != "Tobias")
                .Query;

            Assert.AreEqual("WHERE (((n.AgeLongNullable < {p0}) AND (n.Name = {p1})) OR (n.Name <> {p2}))", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
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
                .Where<FooData>(n => (n.Age < 30 && n.Name == foo.Name) || n.Name != "Tobias")
                .Query;

            Assert.AreEqual("WHERE (((n.Age < {p0}) AND (n.Name = {p1})) OR (n.Name <> {p2}))", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
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
                .Where<FooData>(n => (n.Age < 30 && n.Id == foo.Id) || n.Name != "Tobias")
                .Query;

            Assert.AreEqual("WHERE (((n.Age < {p0}) AND (n.Id = {p1})) OR (n.Name <> {p2}))", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
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
                .Where<FooData>(n => (n.Id == theId))
                .Query;

            Assert.AreEqual("WHERE (n.Id = {p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(777, query.QueryParameters["p0"]);
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
                .Where<FooData>(n => (n.Id == theId))
                .Query;

            Assert.AreEqual("WHERE (n.Id = {p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(777, query.QueryParameters["p0"]);
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
                .Where<FooData>(n => (n.Id == theId))
                .Query;

            Assert.AreEqual("WHERE (n.Id = {p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(777, query.QueryParameters["p0"]);
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
                .Where<FooData>(n => (n.Age < 30 && n.Name == fooData.Name) || n.Name != "Tobias")
                .Query;

            Assert.AreEqual("WHERE (((n.Age < {p0}) AND (n.Name = {p1})) OR (n.Name <> {p2}))", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
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
                .Where<FooData>(n => (n.Age < 30 && n.Id == fooData.Id) || n.Name != "Tobias")
                .Query;

            Assert.AreEqual("WHERE (((n.Age < {p0}) AND (n.Id = {p1})) OR (n.Name <> {p2}))", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
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
                .Where<FooData>(n => (n.Age < 30 && n.Name == "Tobias") || n.Name != "Tobias")
                .Query;

            Assert.AreEqual("WHERE (((n.Age < {p0}) AND (n.Name = {p1})) OR (n.Name <> {p2}))", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
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
                .Where<FooData>(n => n.Age < 30 )
                .Query;

            Assert.AreEqual("WHERE (n.Age < {p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereFilterPropertyExists()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-property-exists
            // START n=node(3, 1)
            // WHERE has(n.Belt)
            // RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where("(has(n.Belt))")
                .Query;

            Assert.AreEqual("WHERE (has(n.Belt))", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
        }

        [Test]
        public void WhereFilterCompareIfPropertyExists()
        {
            // http://docs.neo4j.org/chunked/1.6/query-where.html#where-compare-if-property-exists
            // START n=node(3, 1)
            // WHERE n.Belt = 'white'
            // RETURN n

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(n => n.Id < 30)
                .Query;

            Assert.AreEqual("WHERE (n.Id < {p0})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(30, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereFilterOnNullValues()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(r => r.Name == null && r.Id == 100)
                .Query;

            Assert.AreEqual("WHERE ((not(has(r.Name))) AND (r.Id = {p0}))", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual(100, query.QueryParameters["p0"]);
        }

        [Test]
        public void WhereFilterOnMultipleNodesProperties()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData, BarData>((n1, n2) => n1.Age < 30 && n2.Key == 11)
                .Query;

            Assert.AreEqual("WHERE ((n1.Age < {p0}) AND (n2.Key = {p1}))", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
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
                .Where("(type(r) = {Hosts})")
                .WithParam("Hosts", "HOSTS")
                .Query;

            Assert.AreEqual("WHERE (type(r) = {Hosts})", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters.Count);
            Assert.AreEqual("HOSTS", query.QueryParameters["Hosts"]);
        }

        [Test]
        public void WhereWithAnd()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(n => n.Name == "bob")
                .AndWhere("(type(r) = {Hosts})")
                .WithParam("Hosts", "HOSTS")
                .Query;

            Assert.AreEqual("WHERE (n.Name = {p0})\r\nAND (type(r) = {Hosts})", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual("bob", query.QueryParameters["p0"]);
            Assert.AreEqual("HOSTS", query.QueryParameters["Hosts"]);
        }

        [Test]
        public void WhereWithOr()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(n => n.Name == "bob")
                .OrWhere("(type(r) = {Hosts})")
                .WithParam("Hosts", "HOSTS")
                .Query;

            Assert.AreEqual("WHERE (n.Name = {p0})\r\nOR (type(r) = {Hosts})", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual("bob", query.QueryParameters["p0"]);
            Assert.AreEqual("HOSTS", query.QueryParameters["Hosts"]);
        }

        [Test]
        public void WhereWithOrAnd()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(n => n.Name == "Bob")
                .OrWhere("(type(r) = {Hosts})")
                .WithParam("Hosts", "HOSTS")
                .AndWhere<FooData>(n => n.Id == 10)
                .Query;

            Assert.AreEqual("WHERE (n.Name = {p0})\r\nOR (type(r) = {Hosts})\r\nAND (n.Id = {p2})", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
            Assert.AreEqual("Bob", query.QueryParameters["p0"]);
            Assert.AreEqual(10, query.QueryParameters["p2"]);
            Assert.AreEqual("HOSTS", query.QueryParameters["Hosts"]);
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
                .Where("(a<--b)")
                .Query;

            Assert.AreEqual("WHERE (a<--b)", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
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
                .Where("(n.Name =~ /Tob.*/)")
                .Query;

            Assert.AreEqual("WHERE (n.Name =~ /Tob.*/)", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count);
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
                .Start(new {
                    a = (NodeReference)1,
                    b = (NodeReference)2
                })
                .Create("a-[r:REL]->b")
                .Return<object>("r")
                .Query;

            Assert.AreEqual("START a=node({p0}), b=node({p1})\r\nCREATE a-[r:REL]->b\r\nRETURN r", query.QueryText);
            Assert.AreEqual(1, query.QueryParameters["p0"]);
            Assert.AreEqual(2, query.QueryParameters["p1"]);
        }

        [Test]
        public void CreateNode()
        {
            //http://docs.neo4j.org/chunked/milestone/query-create.html#create-create-single-node-and-set-properties
            // CREATE (a {Foo: 'foo', Bar: 'bar', Baz: 'baz'})
            // RETURN a

            var data = new CreateNodeTests.TestNode {Foo = "foo", Bar = "bar", Baz = "baz"};
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Create("a", data)
                .Return<object>("a")
                .Query;
            Assert.AreEqual("CREATE (a {p0})\r\nRETURN a", query.QueryText);
            Assert.AreEqual(data, query.QueryParameters["p0"]);
        }

        [Test]
        public void CreateAFullPath() {
            //http://docs.neo4j.org/chunked/milestone/query-create.html#create-create-a-full-path
            // START n=node(1)
            // CREATE n-[r:REL]->(a {Foo: 'foo', Bar: 'bar', Baz: 'baz'})-[r:REL]->(b {Foo: 'foo2', Bar: 'bar2', Baz: 'baz2'})
            // RETURN a

            var data1 = new CreateNodeTests.TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var data2 = new CreateNodeTests.TestNode { Foo = "foo2", Bar = "bar2", Baz = "baz2" };
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)1)
                .Create("n-[r:REL]->(a {0})-[r:REL]->(b {1})", data1, data2)
                .Return<CreateNodeTests.TestNode>("a")
                .Query;
            Assert.AreEqual("START n=node({p0})\r\nCREATE n-[r:REL]->(a {p1})-[r:REL]->(b {p2})\r\nRETURN a", query.QueryText);
            Assert.AreEqual(data1, query.QueryParameters["p1"]);
            Assert.AreEqual(data2, query.QueryParameters["p2"]);
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
                .Start(new {
                    a = (NodeReference)1,
                    b = (NodeReference)2
                })
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

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/45/cyper-should-allow-for-flexible-order-of")]
        public void SupportsFlexibleOrderOfClauses()
        {
            // START me=node:node_auto_index(name='Bob')
            // MATCH me-[r?:STATUS]-secondlatestupdate
            // DELETE r
            // WITH me, secondlatestupdate
            // CREATE me-[:STATUS]->(latest_update{text:'Status',date:123})
            // WITH latest_update,secondlatestupdate
            // CREATE latest_update-[:NEXT]-secondlatestupdate
            // WHERE secondlatestupdate <> null
            // RETURN latest_update.text as new_status

            var update = new { text = "Status", date = 123 };

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new { me = Node.ByIndexLookup("node_auto_index", "name", "Bob") })
                .Match("me-[r?:STATUS]-secondlatestupdate")
                .Delete("r")
                .With("me, secondlatestupdate")
                .Create("me-[:STATUS]->(latest_update {update})")
                .WithParams(new { update })
                .With("latest_update,secondlatestupdate")
                .Create("latest_update-[:NEXT]-secondlatestupdate")
                .Where("secondlatestupdate <> null")
// ReSharper disable InconsistentNaming
                .Return(latest_update => new { new_status = latest_update.As<UpdateData>().text })
// ReSharper restore InconsistentNaming
                .Query;

            Assert.AreEqual(@"START me=node:`node_auto_index`(name = {p0})
MATCH me-[r?:STATUS]-secondlatestupdate
DELETE r
WITH me, secondlatestupdate
CREATE me-[:STATUS]->(latest_update {update})
WITH latest_update,secondlatestupdate
CREATE latest_update-[:NEXT]-secondlatestupdate
WHERE secondlatestupdate <> null
RETURN latest_update.text AS new_status", query.QueryText);
            Assert.AreEqual("Bob", query.QueryParameters["p0"]);
            Assert.AreEqual(update, query.QueryParameters["update"]);
        }

        public class UpdateData
        {
// ReSharper disable InconsistentNaming
            public string text { get; set; }
// ReSharper restore InconsistentNaming
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
