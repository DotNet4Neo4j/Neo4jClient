using System;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Tests.GraphClientTests;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQueryTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task ExecutesQuery()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            CypherQuery executedQuery = null;
            client
                .When(c => c.ExecuteCypherAsync(Arg.Any<CypherQuery>()))
                .Do(ci => { executedQuery = ci.Arg<CypherQuery>(); });

            // Act
            await new CypherFluentQuery(client)
                .Match("n")
                .Delete("n")
                .ExecuteWithoutResultsAsync();

            // Assert
            Assert.NotNull(executedQuery);
            Assert.Equal("MATCH n" + Environment.NewLine + "DELETE n", executedQuery.QueryText);
        }

        [Fact]
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
                .Match("n")
                .Delete("n")
                .ExecuteWithoutResultsAsync();
            task.Wait();

            // Assert
            Assert.NotNull(executedQuery);
            Assert.Equal("MATCH n" + Environment.NewLine + "DELETE n", executedQuery.QueryText);
        }

        [Fact]
        public void ShouldBuildQueriesAsImmutableStepsInsteadOfCorruptingPreviousOnes()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Return<object>("n");

            var query1 = query.Query;
            query = query.OrderBy("n.Foo");
            var query2 = query.Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN n", query1.QueryText);
            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN n" + Environment.NewLine + "ORDER BY n.Foo", query2.QueryText);
        }
        
        [Fact]
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

            Assert.Equal("RETURN n" + Environment.NewLine + "LIMIT $p0", query.QueryText);
            Assert.Equal(1L, query.QueryParameters.Count);
            Assert.Equal(3, query.QueryParameters["p0"]);
        }

        [Fact]
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

            Assert.Equal("RETURN n" + Environment.NewLine + "ORDER BY n.name" + Environment.NewLine + "SKIP $p0", query.QueryText);
            Assert.Equal(1L, query.QueryParameters.Count);
            Assert.Equal(3, query.QueryParameters["p0"]);
        }

        [Fact]
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

            Assert.Equal("RETURN n" + Environment.NewLine + "ORDER BY n.name" + Environment.NewLine + "SKIP $p0" + Environment.NewLine + "LIMIT $p1", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(1, query.QueryParameters["p0"]);
            Assert.Equal(2, query.QueryParameters["p1"]);
        }

        [Fact]
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

            Assert.Equal("RETURN n" + Environment.NewLine + "ORDER BY n.length?", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
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

            Assert.Equal("RETURN n" + Environment.NewLine + "ORDER BY n.name", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
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

            Assert.Equal("RETURN n" + Environment.NewLine + "ORDER BY n.age, n.name", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
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

            Assert.Equal("RETURN n" + Environment.NewLine + "ORDER BY n.name DESC", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void OrderNodesByMultiplePropertiesDescending()
        {
            // http://docs.neo4j.org/chunked/stable/query-order.html#order-by-order-nodes-in-descending-order
            // START n=node(3,1,2)
            // RETURN n
            // ORDER BY n.age DESC, n.name DESC

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<object>("n")
                .OrderByDescending("n.age", "n.name")
                .Query;

            Assert.Equal("RETURN n" + Environment.NewLine + "ORDER BY n.age DESC, n.name DESC", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void OrderNodesByMultiplePropertiesWithDifferentOrders()
        {
            // http://docs.neo4j.org/chunked/stable/query-order.html#order-by-order-nodes-by-multiple-properties
            // START n=node(3,1,2)
            // RETURN n
            // ORDER BY n.age, n.name, n.number DESC, n.male DESC, n.lastName

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<object>("n")
                .OrderBy("n.age", "n.name")
                .ThenByDescending("n.number", "n.male")
                .ThenBy("n.lastName")
                .Query;

            Assert.Equal("RETURN n" + Environment.NewLine + "ORDER BY n.age, n.name, n.number DESC, n.male DESC, n.lastName", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void ReturnColumnAlias()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Return(a => new ReturnPropertyQueryResult
                {
                    SomethingTotallyDifferent = a.As<FooData>().Age
                })
                .Query;

            Assert.Equal("MATCH a" + Environment.NewLine + "RETURN a.Age AS SomethingTotallyDifferent", query.QueryText);
        }

        [Fact]
        public void ReturnUniqueResults()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-unique-results
            // START a=node(1)
            // MATCH (a)-->(b)
            // RETURN distinct b

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match("(a)-->(b)")
                .ReturnDistinct<object>("b")
                .Query;

            Assert.Equal("MATCH a" + Environment.NewLine + "MATCH (a)-->(b)" + Environment.NewLine + "RETURN distinct b", query.QueryText);
        }

        [Fact]
        public void ReturnUniqueResultsWithExpression()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-unique-results
            // START a=node(1)
            // MATCH (a)-->(b)
            // RETURN distinct b

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match("(a)-->(b)")
                .ReturnDistinct(b => new FooData
                {
                    Age = b.As<FooData>().Age
                })
                .Query;

            Assert.Equal("MATCH a" + Environment.NewLine + "MATCH (a)-->(b)" + Environment.NewLine + "RETURN distinct b.Age AS Age", query.QueryText);
        }

        [Fact]
        public void ReturnPropertiesIntoAnonymousType()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match("(a)-->(b)")
                .Return(b => new
                {
                    SomeAge = b.As<FooData>().Age,
                    SomeName = b.As<FooData>().Name
                })
                .Query;

            string expected = string.Format("MATCH a{0}MATCH (a)-->(b){0}RETURN b.Age AS SomeAge, b.Name AS SomeName", Environment.NewLine);

            Assert.Equal(expected.TrimStart('\r', '\n'), query.QueryText);
            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
        }

        [Fact]
        public void ReturnPropertiesIntoAnonymousTypeWithAutoNames()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match("(a)-->(b)")
                .Return(b => new
                {
                    b.As<FooData>().Age,
                    b.As<FooData>().Name
                })
                .Query;

            string expected = string.Format("MATCH a{0}MATCH (a)-->(b){0}RETURN b.Age AS Age, b.Name AS Name", Environment.NewLine);

            Assert.Equal(expected.TrimStart('\r', '\n'), query.QueryText);
            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
        }

        [Fact]
        public void ReturnPropertiesFromMultipleNodesIntoAnonymousTypeWithAutoNames()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match("(a)-->(b)-->(c)")
                .Return((b, c) => new
                {
                    b.As<FooData>().Age,
                    c.As<FooData>().Name
                })
                .Query;

            string expected = "MATCH a" + Environment.NewLine + "MATCH (a)-->(b)-->(c)" + Environment.NewLine + "RETURN b.Age AS Age, c.Name AS Name";

            Assert.Equal(expected.TrimStart('\r', '\n'), query.QueryText);
            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
        }

        [Fact]
        public void ReturnNodeDataIntoAnonymousType()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match("(a)-->(b)")
                .Return((b, c) => new
                {
                    NodeB = b.As<FooData>(),
                })
                .Query;

            string expected = "MATCH a" + Environment.NewLine + "MATCH (a)-->(b)" + Environment.NewLine + "RETURN b AS NodeB";

            Assert.Equal(expected.TrimStart('\r', '\n'), query.QueryText);
            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
        }

        [Fact]
        public void ReturnEntireNodeDataAndReferenceIntoAnonymousType()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match("(a)-->(b)")
                .Return((b, c) => new
                {
                    NodeB = b.Node<FooData>(),
                })
                .Query;

            string expected = string.Format("MATCH a{0}MATCH (a)-->(b){0}RETURN b AS NodeB", Environment.NewLine);

            Assert.Equal(expected.TrimStart('\r', '\n'), query.QueryText);
            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
        }

        [Fact]
        public void ReturnEntireNodeDataAndReferenceIntoProjectionType()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match("(a)-->(b)")
                .Return((b, c) => new ReturnEntireNodeDataAndReferenceIntoProjectionTypeResult
                {
                    NodeB = b.Node<FooData>(),
                })
                .Query;

            string expected = string.Format("MATCH a{0}MATCH (a)-->(b){0}RETURN b AS NodeB", Environment.NewLine);

            Assert.Equal(expected.TrimStart('\r', '\n'), query.QueryText);
            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
        }

        public class ReturnEntireNodeDataAndReferenceIntoProjectionTypeResult
        {
            public Node<FooData> NodeB { get; set; }
        }

        [Fact]
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

            Assert.Equal("WHERE (((n.Age < $p0) AND (n.Name = $p1)) OR (n.Name <> $p2))", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
            Assert.Equal("Tobias", query.QueryParameters["p1"]);
            Assert.Equal("Tobias", query.QueryParameters["p2"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (((n.AgeLong < $p0) AND (n.Name = $p1)) OR (n.Name <> $p2))".Replace("'", "\""), query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(30L, query.QueryParameters["p0"]);
            Assert.Equal("Tobias", query.QueryParameters["p1"]);
            Assert.Equal("Tobias", query.QueryParameters["p2"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (((n.AgeLongNullable < $p0) AND (n.Name = $p1)) OR (n.Name <> $p2))", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
            Assert.Equal("Tobias", query.QueryParameters["p1"]);
            Assert.Equal("Tobias", query.QueryParameters["p2"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (((n.Age < $p0) AND (n.Name = $p1)) OR (n.Name <> $p2))", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
            Assert.Equal("Tobias", query.QueryParameters["p1"]);
            Assert.Equal("Tobias", query.QueryParameters["p2"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (((n.Age < $p0) AND (n.Id = $p1)) OR (n.Name <> $p2))", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
            Assert.Equal(777, query.QueryParameters["p1"]);
            Assert.Equal("Tobias", query.QueryParameters["p2"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (n.Id = $p0)", query.QueryText);
            Assert.Equal(1L, query.QueryParameters.Count);
            Assert.Equal(777, query.QueryParameters["p0"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (n.Id = $p0)", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(777L, query.QueryParameters["p0"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (n.Id = $p0)", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
            Assert.Equal(777L, query.QueryParameters["p0"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (((n.Age < $p0) AND (n.Name = $p1)) OR (n.Name <> $p2))", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
            Assert.Equal("Tobias", query.QueryParameters["p1"]);
            Assert.Equal("Tobias", query.QueryParameters["p2"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (((n.Age < $p0) AND (n.Id = $p1)) OR (n.Name <> $p2))", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
            Assert.Equal(777, query.QueryParameters["p1"]);
            Assert.Equal("Tobias", query.QueryParameters["p2"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (((n.Age < $p0) AND (n.Name = $p1)) OR (n.Name <> $p2))", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
            Assert.Equal("Tobias", query.QueryParameters["p1"]);
            Assert.Equal("Tobias", query.QueryParameters["p2"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (n.Age < $p0)", query.QueryText);
            Assert.Equal(1L, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (has(n.Belt))", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
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

            Assert.Equal("WHERE (n.Id < $p0)", query.QueryText);
            Assert.Equal(1L, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
        }

        [Fact]
        public void WhereFilterOnNullValues()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(r => r.Name == null && r.Id == 100)
                .Query;

            Assert.Equal("WHERE ((not(has(r.Name))) AND (r.Id = $p0))", query.QueryText);
            Assert.Equal(1L, query.QueryParameters.Count);
            Assert.Equal(100, query.QueryParameters["p0"]);
        }

        [Fact]
        public void WhereFilterOnMultipleNodesProperties()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData, BarData>((n1, n2) => n1.Age < 30 && n2.Key == 11)
                .Query;

            Assert.Equal("WHERE ((n1.Age < $p0) AND (n2.Key = $p1))", query.QueryText);
            Assert.Equal(2L, query.QueryParameters.Count);
            Assert.Equal(30, query.QueryParameters["p0"]);
            Assert.Equal(11, query.QueryParameters["p1"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (type(r) = {Hosts})", query.QueryText);
            Assert.Equal(1L, query.QueryParameters.Count);
            Assert.Equal("HOSTS", query.QueryParameters["Hosts"]);
        }

        [Fact]
        public void WhereWithAnd()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(n => n.Name == "bob")
                .AndWhere("(type(r) = {Hosts})")
                .WithParam("Hosts", "HOSTS")
                .Query;

            Assert.Equal("WHERE (n.Name = $p0)" + Environment.NewLine + "AND (type(r) = {Hosts})", query.QueryText);
            Assert.Equal(2L, query.QueryParameters.Count);
            Assert.Equal("bob", query.QueryParameters["p0"]);
            Assert.Equal("HOSTS", query.QueryParameters["Hosts"]);
        }

        [Fact]
        public void WhereWithOr()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(n => n.Name == "bob")
                .OrWhere("(type(r) = {Hosts})")
                .WithParam("Hosts", "HOSTS")
                .Query;

            Assert.Equal("WHERE (n.Name = $p0)" + Environment.NewLine + "OR (type(r) = {Hosts})", query.QueryText);
            Assert.Equal(2L, query.QueryParameters.Count);
            Assert.Equal("bob", query.QueryParameters["p0"]);
            Assert.Equal("HOSTS", query.QueryParameters["Hosts"]);
        }

        [Fact]
        public void WhereWithOrAnd()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Where<FooData>(n => n.Name == "Bob")
                .OrWhere("(type(r) = {Hosts})")
                .WithParam("Hosts", "HOSTS")
                .AndWhere<FooData>(n => n.Id == 10)
                .Query;

            Assert.Equal("WHERE (n.Name = $p0)" + Environment.NewLine + "OR (type(r) = {Hosts})" + Environment.NewLine + "AND (n.Id = $p2)", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal("Bob", query.QueryParameters["p0"]);
            Assert.Equal(10, query.QueryParameters["p2"]);
            Assert.Equal("HOSTS", query.QueryParameters["Hosts"]);
        }

        [Fact]
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

            Assert.Equal("WHERE (a<--b)", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
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

            Assert.Equal("WHERE (n.Name =~ /Tob.*/)", query.QueryText);
            Assert.Equal(0, query.QueryParameters.Count);
        }

        [Fact]
        public void CreateRelationshipBetweenTwoNodes()
        {
            //http://docs.neo4j.org/chunked/1.8.M06/query-create.html#create-create-a-relationship-between-two-nodes
            // START a=node(1), b=node(2)
            // CREATE a-[r:REL]->b
            // RETURN r

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a,b")
                .Create("a-[r:REL]->b")
                .Return<object>("r")
                .Query;

            Assert.Equal("MATCH a,b" + Environment.NewLine + "CREATE a-[r:REL]->b" + Environment.NewLine + "RETURN r", query.QueryText);
        }

        [Fact]
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
            Assert.Equal("CREATE (a $p0)" + Environment.NewLine + "RETURN a", query.QueryText);
            Assert.Equal(data, query.QueryParameters["p0"]);
        }

        [Fact]
        public void CreateAFullPath() {
            //http://docs.neo4j.org/chunked/milestone/query-create.html#create-create-a-full-path
            // START n=node(1)
            // CREATE n-[r:REL]->(a {Foo: 'foo', Bar: 'bar', Baz: 'baz'})-[r:REL]->(b {Foo: 'foo2', Bar: 'bar2', Baz: 'baz2'})
            // RETURN a

            var data1 = new CreateNodeTests.TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var data2 = new CreateNodeTests.TestNode { Foo = "foo2", Bar = "bar2", Baz = "baz2" };
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Create("n-[r:REL]->(a $p0)-[r:REL]->(b $p1)")
                .WithParams(new {p0=data1, p1=data2 })
                .Return<CreateNodeTests.TestNode>("a")
                .Query;
            Assert.Equal("MATCH n" + Environment.NewLine + "CREATE n-[r:REL]->(a $p0)-[r:REL]->(b $p1)" + Environment.NewLine + "RETURN a", query.QueryText);
            Assert.Equal(data1, query.QueryParameters["p0"]);
            Assert.Equal(data2, query.QueryParameters["p1"]);
        }

        [Fact]
        public void CreateRelationshipAndSetProperties()
        {
            //http://docs.neo4j.org/chunked/1.8.M06/query-create.html#create-create-a-relationship-and-set-properties
            //START a=node(1), b=node(2)
            //CREATE a-[r:REL {name : a.name + '<->' + b.name }]->b
            //RETURN r

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a,b")
                .Create("a-[r:REL {name : a.name + '<->' + b.name }]->b")
                .Return<object>("r")
                .Query;

            Assert.Equal("MATCH a,b" + Environment.NewLine + "CREATE a-[r:REL {name : a.name + '<->' + b.name }]->b" + Environment.NewLine + "RETURN r", query.QueryText);
        }

        [Fact]
        public void ComplexMatching()
        {
            // http://docs.neo4j.org/chunked/1.8.M03/query-match.html#match-complex-matching
            // START a=node(3)
            // MATCH (a)-[:KNOWS]->(b)-[:KNOWS]->(c), (a)-[:BLOCKS]-(d)-[:KNOWS]-(c)
            // RETURN a,b,c,d

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("a")
                .Match(
                    "(a)-[:KNOWS]->(b)-[:KNOWS]->(c)",
                    "(a)-[:BLOCKS]-(d)-[:KNOWS]-(c)")
                .Query;

            Assert.Equal("MATCH a" + Environment.NewLine + "MATCH (a)-[:KNOWS]->(b)-[:KNOWS]->(c), (a)-[:BLOCKS]-(d)-[:KNOWS]-(c)", query.QueryText);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/45/cyper-should-allow-for-flexible-order-of")]
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
                .Match("me")
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

            Assert.Equal(string.Format("MATCH me{0}MATCH me-[r?:STATUS]-secondlatestupdate{0}DELETE r{0}WITH me, secondlatestupdate{0}CREATE me-[:STATUS]->(latest_update {{update}}){0}WITH latest_update,secondlatestupdate{0}CREATE latest_update-[:NEXT]-secondlatestupdate{0}WHERE secondlatestupdate <> null{0}RETURN latest_update.text AS new_status", Environment.NewLine), query.QueryText);
            Assert.Equal(update, query.QueryParameters["update"]);
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
