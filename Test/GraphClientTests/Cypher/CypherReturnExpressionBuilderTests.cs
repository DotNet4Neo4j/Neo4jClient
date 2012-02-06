using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class CypherReturnExpressionBuilderTests
    {
        [Test]
        public void ReturnProperty()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var text = CypherReturnExpressionBuilder.BuildText(a => new ReturnPropertyQueryResult
            {
                SomethingTotallyDifferent = a.As<Foo>().Age
            });

            Assert.AreEqual("a.Age AS SomethingTotallyDifferent", text);
        }

        [Test]
        public void ReturnMultipleProperties()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent, a.Name as FirstName

            var text = CypherReturnExpressionBuilder.BuildText(a => new ReturnPropertyQueryResult
            {
                SomethingTotallyDifferent = a.As<Foo>().Age,
                FirstName = a.As<Foo>().Name
            });

            Assert.AreEqual("a.Age AS SomethingTotallyDifferent, a.Name AS FirstName", text);
        }

        [Test]
        public void NullablePropertiesShouldBeQueriedAsCypherOptionalProperties()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-optional-properties
            // START n=node(1)
            // RETURN n.Age AS Age, n.NumberOfCats? AS NumberOfCats

            var text = CypherReturnExpressionBuilder.BuildText(n => new OptionalPropertiesQueryResult
            {
                Age = n.As<Foo>().Age,
                NumberOfCats = n.As<Foo>().NumberOfCats
            });

            Assert.AreEqual("n.Age AS Age, n.NumberOfCats? AS NumberOfCats", text);
        }

        public class Foo
        {
            public int Age { get; set; }
            public string Name { get; set; }
            public int? NumberOfCats { get; set; }
        }

        public class ReturnPropertyQueryResult
        {
            public int SomethingTotallyDifferent { get; set; }
            public string FirstName { get; set; }
        }

        public class OptionalPropertiesQueryResult
        {
            public int Age { get; set; }
            public int? NumberOfCats { get; set; }
        }
    }
}
