﻿using System;
using System.Collections.Generic;
using System.Linq;
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("a.Age AS SomethingTotallyDifferent", returnExpression.Text);
        }

        [Test]
        public void ReturnNode()
        {            
            Expression<Func<ICypherResultItem, object>> expression =
                a => new 
                {
                    FooNode = a.As<Node<Foo>>()
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("a AS FooNode", returnExpression.Text);
        }
        
        [Test]
        public void ReturnPropertyWithNullablePropertyOnRightHandSide()
        {
            Expression<Func<ICypherResultItem, Foo>> expression =
                a => new Foo
                {
                    Age = a.As<Foo>().AgeNullable.Value
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("a.AgeNullable AS Age", returnExpression.Text);
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("a.Age AS SomethingTotallyDifferent, a.Name AS FirstName", returnExpression.Text);
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("a.Age AS SomethingTotallyDifferent, a.Name AS FirstName", returnExpression.Text);
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("john.Age AS SomethingTotallyDifferent, fof.Name AS FirstName", returnExpression.Text);
        }

        [Test]
        public void NullablePropertiesShouldBeQueriedAsCypherOptionalProperties_PreNeo4j20()
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Cypher19, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("n.Age AS Age, n.NumberOfCats? AS NumberOfCats", returnExpression.Text);
        }

        [Test]
        public void NullablePropertiesShouldNotGetSpecialHandling()
        {
            Expression<Func<ICypherResultItem, OptionalPropertiesQueryResult>> expression =
                n => new OptionalPropertiesQueryResult
                {
                    Age = n.As<Foo>().Age,
                    NumberOfCats = n.As<Foo>().NumberOfCats
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("n.Age AS Age, n.NumberOfCats AS NumberOfCats", returnExpression.Text);
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("a AS Foo", returnExpression.Text);
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("a AS Foo, b AS Bar", returnExpression.Text);
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("collect(a) AS Foo", returnExpression.Text);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/118/collectas-causes-argumentnullexception")]
        public void PreventDoubleNodeWrappedCollectAs()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    Foo = a.CollectAs<Node<Foo>>()
                };

            var ex = Assert.Throws<ArgumentException>(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
            StringAssert.StartsWith(CypherReturnExpressionBuilder.CollectAsShouldNotBeNodeTExceptionMessage, ex.Message);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/118/collectas-causes-argumentnullexception")]
        public void PreventDoubleNodeWrappedCollectAsDistinct()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    Foo = a.CollectAsDistinct<Node<Foo>>()
                };

            var ex = Assert.Throws<ArgumentException>(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
            StringAssert.StartsWith(CypherReturnExpressionBuilder.CollectAsDistinctShouldNotBeNodeTExceptionMessage, ex.Message);
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

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("collect(distinct a) AS Foo", returnExpression.Text);
        }

        [Test]
        public void ReturnHeadCollectedNodesInColumn()
        {
            // http://docs.neo4j.org/chunked/milestone/query-functions-scalar.html#functions-head
            // START a=node(1)
            // MATCH a<--b
            // RETURN head(collect(a))

            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    Foo = a.Head().CollectAs<Foo>()
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("head(collect(a)) AS Foo", returnExpression.Text);
        }

        [Test]
        public void ReturnLastCollectedNodesInColumn()
        {
            // http://docs.neo4j.org/chunked/milestone/query-functions-scalar.html#functions-last
            // START a=node(1)
            // MATCH a<--b
            // RETURN last(collect(a))

            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    Foo = a.Last().CollectAs<Foo>()
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("last(collect(a)) AS Foo", returnExpression.Text);
        }

        [Test]
        public void ReturnCountInAnonymousType()
        {
            // http://docs.neo4j.org/chunked/1.8.M05/query-aggregation.html#aggregation-collect
            // START a=node(1)
            // MATCH a<--b
            // RETURN count(b)

            Expression<Func<ICypherResultItem, object>> expression =
                b => new
                {
                    Foo = b.Count()
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("count(b) AS Foo", returnExpression.Text);
        }

        [Test]
        public void ReturnCountOnItsOwn()
        {
            // http://docs.neo4j.org/chunked/1.8.M05/query-aggregation.html#aggregation-collect
            // START a=node(1)
            // MATCH a<--b
            // RETURN count(b)

            Expression<Func<ICypherResultItem, long>> expression = b => b.Count();

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("count(b)", returnExpression.Text);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/116/bug-in-returning-single-nullable-value")]
        public void ReturnCountOnItsOwnAsNullableLong()
        {
            Expression<Func<ICypherResultItem, long?>> expression = b => b.Count();
            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);
            Assert.AreEqual("count(b)", returnExpression.Text);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/165/as-throws-systemargumentexception-in")]
        public void ReturnComplexAnonymousWithValueTypesAndCustomExpressions()
        {
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, object>> expression = (ping, reviews, reviewer) => new
            {
                PingId = Return.As<long>("ping.Id"),
                PingImage = Return.As<string>("ping.Image"),
                PingDescription = Return.As<string>("ping.Description"),
                Reviews = reviews.As<long>(),
                Reviewers = reviewer.CountDistinct(),
                Avatars = Return.As<IEnumerable<string>>("collect(distinct reviewer.Avatar)[0..{maxAvatars}]")
            };
            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);
            Assert.AreEqual("ping.Id AS PingId, ping.Image AS PingImage, ping.Description AS PingDescription, reviews AS Reviews, count(distinct reviewer) AS Reviewers, collect(distinct reviewer.Avatar)[0..{maxAvatars}] AS Avatars", returnExpression.Text);
        }

        [Test]
        [Description("https://github.com/Readify/Neo4jClient/pull/56#issuecomment-44158504")]
        public void ReturnComplexTupleWithValueTypesAndCustomExpressions()
        {
            Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, object>> expression = (ping, reviews, reviewer) => new
            Tuple<long, string, string, long, long, IEnumerable<string>>(
                Return.As<long>("ping.Id"),
                Return.As<string>("ping.Image"),
                Return.As<string>("ping.Description"),
                reviews.As<long>(),
                reviewer.CountDistinct(),
                Return.As<IEnumerable<string>>("collect(distinct reviewer.Avatar)[0..{maxAvatars}]")
            );
            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);
            Assert.AreEqual("ping.Id AS Item1, ping.Image AS Item2, ping.Description AS Item3, reviews AS Item4, count(distinct reviewer) AS Item5, collect(distinct reviewer.Avatar)[0..{maxAvatars}] AS Item6", returnExpression.Text);
        }

        [Test]
        public void ReturnLabelsInAnonymousType()
        {
            // MATCH (a:User)
            // WHERE a.Id == 123
            // RETURN labels(a)

            Expression<Func<ICypherResultItem, object>> expression =
                b => new
                {
                    Foo = b.Labels()
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("labels(b) AS Foo", returnExpression.Text);
        }

        [Test]
        public void ReturnAllOnItsOwn()
        {
            Expression<Func<long>> expression = () => All.Count();
            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);
            Assert.AreEqual("count(*)", returnExpression.Text);
        }

        [Test]
        public void ReturnCustomStatementOnItsOwn()
        {
            Expression<Func<long>> expression = () => Return.As<long>("custom statement");
            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);
            Assert.AreEqual("custom statement", returnExpression.Text);
        }

        [Test]
        public void ReturnCustomCypherTextFromConstant()
        {
            // START a=node(1)
            // MATCH a<--b
            // RETURN abs(sum(a.age) - sum(b.age))

            Expression<Func<ICypherResultItem, object>> expression =
                b => new
                {
                    Foo = Return.As<long>("abs(sum(a.age) - sum(b.age))")
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("abs(sum(a.age) - sum(b.age)) AS Foo", returnExpression.Text);
        }

        [Test]
        public void ReturnCustomCypherTextFromDynamicCode()
        {
            // START a=node(1)
            // MATCH a<--b
            // RETURN abs(sum(a.age) - sum(b.age))

            Expression<Func<ICypherResultItem, object>> expression =
                b => new
                {
                    Foo = Return.As<long>("abs(sum(a.age)" + new string(' ', 1) + "- sum(b.age))")
                };

            var returnExpression = CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters);

            Assert.AreEqual("abs(sum(a.age) - sum(b.age)) AS Foo", returnExpression.Text);
        }

        [Test]
        public void ThrowNiceErrorForChainedMethods()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => new
                {
                    Foo = a
                        .CollectAs<Foo>()
                        .Select(f => f.Data)
                        .ToList()
                };

            var ex = Assert.Throws<ArgumentException>(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
            Assert.AreEqual(CypherReturnExpressionBuilder.ReturnExpressionCannotBeSerializedToCypherExceptionMessage, ex.Message);
        }

        [Test]
        public void ThrowNiceErrorForStructInNewExpression()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => new KeyValuePair<ICypherResultItem, ICypherResultItem>();
            
            var ex = Assert.Throws<ArgumentException>(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
            StringAssert.StartsWith(CypherReturnExpressionBuilder.ReturnExpressionCannotBeStruct, ex.Message);
        }

        [Test]
        public void ThrowNiceErrorForStructInMemberInitExpression()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => new KeyValuePair<ICypherResultItem, ICypherResultItem>()
                {
                };

            var ex = Assert.Throws<ArgumentException>(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
            StringAssert.StartsWith(CypherReturnExpressionBuilder.ReturnExpressionCannotBeStruct, ex.Message);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/47/problem-casting-cypher-query-results-to")]
        public void ThrowNiceErrorForConstructorsWithArguments()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => new KeyValuePair<ICypherResultItem, ICypherResultItem>(a, a);

            var ex = Assert.Throws<ArgumentException>(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
            StringAssert.StartsWith(CypherReturnExpressionBuilder.ReturnExpressionShouldBeOneOfExceptionMessage, ex.Message);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/159/problems-with-nodebyindexlookup")]
        public void ThrowNiceErrorForConstructorsWithArgumentsInReturnAs()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => a.As<TypeWithoutDefaultConstructor>();

            var ex = Assert.Throws<ArgumentException>(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));

            const string expectedMessage =
                "You've called As<TypeWithoutDefaultConstructor>() in your return clause, where TypeWithoutDefaultConstructor is not a supported type. It must be a simple type (like int, string, or long), a class with a default constructor (so that we can deserialize into it), RelationshipInstance, RelationshipInstance<T>, list of RelationshipInstance, or list of RelationshipInstance<T>.";
            StringAssert.StartsWith(expectedMessage, ex.Message);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/171/remove-false-protection-around-return-a")]
        public void AllowArrayOfRelationshipInstanceInReturnAs()
        {
            Expression<Func<ICypherResultItem, object>> expression = a => a.As<RelationshipInstance[]>();
            Assert.DoesNotThrow(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/171/remove-false-protection-around-return-a")]
        public void AllowListOfRelationshipInstanceInReturnAs()
        {
            Expression<Func<ICypherResultItem, object>> expression = a => a.As<List<RelationshipInstance>>();
            Assert.DoesNotThrow(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/171/remove-false-protection-around-return-a")]
        public void AllowEnumerableOfRelationshipInstanceInReturnAs()
        {
            Expression<Func<ICypherResultItem, object>> expression = a => a.As<IEnumerable<RelationshipInstance<object>>>();
            Assert.DoesNotThrow(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/171/remove-false-protection-around-return-a")]
        public void AllowArrayOfInt32InReturnAs()
        {
            Expression<Func<ICypherResultItem, object>> expression = a => a.As<int[]>();
            Assert.DoesNotThrow(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/171/remove-false-protection-around-return-a")]
        public void AllowEnumerableOfInt32InReturnAs()
        {
            Expression<Func<ICypherResultItem, object>> expression = a => a.As<IEnumerable<int>>();
            Assert.DoesNotThrow(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/171/remove-false-protection-around-return-a")]
        public void AllowListOfInt32InReturnAs()
        {
            Expression<Func<ICypherResultItem, object>> expression = a => a.As<List<int>>();
            Assert.DoesNotThrow(() => CypherReturnExpressionBuilder.BuildText(expression, CypherCapabilities.Default, GraphClient.DefaultJsonConverters));
        }

        public class TypeWithoutDefaultConstructor
        {
            readonly int c;

            public TypeWithoutDefaultConstructor(int a, int b)
            {
                c = a + b;
            }

            public override string ToString()
            {
                return c.ToString();
            }
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
