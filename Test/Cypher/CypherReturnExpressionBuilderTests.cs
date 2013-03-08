using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
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

            Expression<Func<ICypherResultItem, ReturnPropertyQueryResult>> expression =
                a => new ReturnPropertyQueryResult
                {
                    SomethingTotallyDifferent = a.As<Foo>().Age
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("a.Age AS SomethingTotallyDifferent", text);
        }

        [Test]
        public void ReturnPropertyWithNullablePropertyOnRightHandSide()
        {
            Expression<Func<ICypherResultItem, Foo>> expression =
                a => new Foo
                {
                    Age = a.As<Foo>().AgeNullable.Value
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("a.AgeNullable? AS Age", text);
        }

        [Test]
        public void ReturnMultipleProperties()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent, a.Name as FirstName

            Expression<Func<ICypherResultItem, ReturnPropertyQueryResult>> expression =
                a => new ReturnPropertyQueryResult
                {
                    SomethingTotallyDifferent = a.As<Foo>().Age,
                    FirstName = a.As<Foo>().Name
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("a.Age AS SomethingTotallyDifferent, a.Name? AS FirstName", text);
        }

        [Test]
        public void ReturnMultiplePropertiesInAnonymousType()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent, a.Name as FirstName

            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    SomethingTotallyDifferent = a.As<Foo>().Age,
                    FirstName = a.As<Foo>().Name
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("a.Age AS SomethingTotallyDifferent, a.Name? AS FirstName", text);
        }

        [Test]
        public void ReturnMultiplePropertiesFromMultipleColumns()
        {
            // http://docs.neo4j.org/chunked/milestone/cypher-query-lang.html
            // START john=node(1)
            // MATCH john-[:friend]->()-[:friend]->fof
            // RETURN john.Age, fof.Name

            Expression<Func<ICypherResultItem, ICypherResultItem, ReturnPropertyQueryResult>> expression =
                (john, fof) => new ReturnPropertyQueryResult
                {
                    SomethingTotallyDifferent = john.As<Foo>().Age,
                    FirstName = fof.As<Foo>().Name
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("john.Age AS SomethingTotallyDifferent, fof.Name? AS FirstName", text);
        }

        [Test]
        public void NullablePropertiesShouldBeQueriedAsCypherOptionalProperties()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-optional-properties
            // START n=node(1)
            // RETURN n.Age AS Age, n.NumberOfCats? AS NumberOfCats

            Expression<Func<ICypherResultItem, OptionalPropertiesQueryResult>> expression =
                n => new OptionalPropertiesQueryResult
                {
                    Age = n.As<Foo>().Age,
                    NumberOfCats = n.As<Foo>().NumberOfCats
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("n.Age AS Age, n.NumberOfCats? AS NumberOfCats", text);
        }

        [Test]
        public void ReturnNodeInColumn()
        {
            // START a=node(1)
            // RETURN a AS Foo

            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    Foo = a.As<Foo>()
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("a AS Foo", text);
        }

        [Test]
        public void ReturnMultipleNodesInColumns()
        {
            // START a=node(1)
            // MATCH a<--b
            // RETURN a AS Foo, b AS Bar

            Expression<Func<ICypherResultItem, ICypherResultItem, object>> expression =
                (a, b) => new
                {
                    Foo = a.As<Foo>(),
                    Bar = b.As<Foo>()
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("a AS Foo, b AS Bar", text);
        }

        [Test]
        public void ReturnCollectedNodesInColumn()
        {
            // http://docs.neo4j.org/chunked/1.8.M05/query-aggregation.html#aggregation-collect
            // START a=node(1)
            // MATCH a<--b
            // RETURN collect(a)

            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    Foo = a.CollectAs<Foo>()
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("collect(a) AS Foo", text);
        }

        [Test]
        public void ReturnCollectedDistinctNodesInColumn()
        {
            // http://docs.neo4j.org/chunked/1.9.M05/query-aggregation.html#aggregation-distinct
            // START a=node(1)
            // MATCH a-->b
            // RETURN collect(distinct b.eyes)

            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    Foo = a.CollectAsDistinct<Foo>()
                };

            var text = CypherReturnExpressionBuilder.BuildText(expression);

            Assert.AreEqual("collect(distinct a) AS Foo", text);
        }

        public class Foo
        {
            public int Age { get; set; }
            public int? AgeNullable { get; set; }
            public string Name { get; set; }
            public int NumberOfCats { get; set; }
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
