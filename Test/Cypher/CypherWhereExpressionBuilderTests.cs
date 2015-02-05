using NUnit.Framework;
using Neo4jClient.Cypher;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;

namespace Neo4jClient.Test.Cypher
{
    public class CypherWhereExpressionBuilderTests
    {
        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        class Foo
        {
            public int Bar { get; set; }
            public int? NullableBar { get; set; }
            public bool SomeBool { get; set; }
        }

        class Nested
        {
            public Foo Foo { get; set; }
        }

        class SuperNested
        {
            public Nested Nested { get; set; }
        }
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        // This must be a public static field, that's not a constant
        public static int BazField = 123;

        // This must be a public static property
        public static int BazProperty
        {
            get { return 456; }
        }

        interface IFoo
        {
            int Bar { get; set; }
        }

        [Test]
        public void AccessStaticField()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.Bar == BazField;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.Bar = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void AccessStaticProperty()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.Bar == BazProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.Bar = {p0})", result);
            Assert.AreEqual(456, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/158/neo4jclient-cypher-where-clauses-using-a")]
        public void ShouldCompareAgainstValueOfNullableType()
        {
            var bar = (long?)123;

            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.Bar == bar.Value;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.Bar = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void ForPre20VersionsEvaluateTrueWhenComparingMissingNullablePropertyToNotConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar? <> {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/103/cypher-queries-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyGreaterThanConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar > 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! > {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/103/cypher-queries-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyGreaterThanOrEqualToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar >= 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! >= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/103/cypher-queries-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyLessThanConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar < 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! < {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/103/cypher-queries-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyLessThanOrEqualToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar <= 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! <= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void ForPre20VersionsEvaluateTrueWhenComparingMissingNullablePropertyToNullProperty()
        {
            var parameters = new Dictionary<string, object>();
            var fooWithNulls = new Foo
            {
                NullableBar = null
            };
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == fooWithNulls.NullableBar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar? is null)", result);
        }

        [Test]
        public void For20VersionsEvaluateTrueWhenComparingMissingNullablePropertyToNullProperty()
        {
            var parameters = new Dictionary<string, object>();
            var fooWithNulls = new Foo
            {
                NullableBar = null
            };
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == fooWithNulls.NullableBar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher203);

            Assert.AreEqual("(not(has(foo.NullableBar)))", result);
        }

        [Test]
        public void ForPre20VersionsEvaluateTrueWhenComparingNotMissingNullablePropertyToNullProperty()
        {
            var parameters = new Dictionary<string, object>();
            var fooWithNulls = new Foo
            {
                NullableBar = null
            };
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != fooWithNulls.NullableBar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar? is not null)", result);
        }

        [Test]
        public void For20VersionsEvaluateTrueWhenComparingNotMissingNullablePropertyToNullProperty()
        {
            var parameters = new Dictionary<string, object>();
            var fooWithNulls = new Foo
            {
                NullableBar = null
            };
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != fooWithNulls.NullableBar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher203);

            Assert.AreEqual("(has(foo.NullableBar))", result);
        }

        [Test]
        public void ForPre20VersionsEvaluateTrueWhenComparingMissingNullablePropertyToNull()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == null;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar? is null)", result);
        }

        [Test]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyToNotNull()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != null;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar? is not null)", result);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyToLocalMemberValue()
        {
            var localObject = new {NoneCypherLocalProperty = 123};
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateTrueWhenComparingMissingNullablePropertyToNotLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar? <> {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyGreaterThanLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar > localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! > {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyGreaterThanOrEqualToLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar >= localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! >= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyLessThanLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar < localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! < {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void ForPre20VersionsEvaluateFalseWhenComparingMissingNullablePropertyLessThanOrEqualToLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar <= localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v), CypherCapabilities.Cypher190);

            Assert.AreEqual("(foo.NullableBar! <= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyToNotConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar <> {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyGreaterThanConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar > 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar > {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyGreaterThanOrEqualToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar >= 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar >= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyLessThanConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar < 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar < {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyLessThanOrEqualToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar <= 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar <= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyToNull()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == null;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(not(has(foo.NullableBar)))", result);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyToNotNull()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != null;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(has(foo.NullableBar))", result);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyToLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyToNotLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar <> {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyGreaterThanLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar > localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar > {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyGreaterThanOrEqualToLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar >= localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar >= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyLessThanLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar < localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar < {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/163/neo4j-v2m6-client-syntax-error")]
        public void DontSuffixPropertyWhenComparingMissingNullablePropertyLessThanOrEqualToLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar <= localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar <= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void ShouldComparePropertiesAcrossEntities()
        {
            // http://stackoverflow.com/questions/15718916/neo4jclient-where-clause-not-putting-in-parameters
            // Where<TSourceNode, TSourceNode>((otherStartNodes, startNode) => otherStartNodes.Id != startNode.Id)

            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, Foo, bool>> expression =
                (p1, p2) => p1.Bar == p2.Bar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(p1.Bar = p2.Bar)", result);
        }

        [Test]
        public void ShouldComparePropertiesAcrossEntitiesNotEqual()
        {
            // http://stackoverflow.com/questions/15718916/neo4jclient-where-clause-not-putting-in-parameters
            // Where<TSourceNode, TSourceNode>((otherStartNodes, startNode) => otherStartNodes.Id != startNode.Id)

            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, Foo, bool>> expression =
                (p1, p2) => p1.Bar != p2.Bar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(p1.Bar <> p2.Bar)", result);
        }

        [Test]
        public void ShouldComparePropertiesAcrossInterfaces()
        {
            // http://stackoverflow.com/questions/15718916/neo4jclient-where-clause-not-putting-in-parameters
            // Where<TSourceNode, TSourceNode>((otherStartNodes, startNode) => otherStartNodes.Id != startNode.Id)

            var parameters = new Dictionary<string, object>();
            Expression<Func<IFoo, IFoo, bool>> expression =
                (p1, p2) => p1.Bar == p2.Bar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(p1.Bar = p2.Bar)", result);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/73/where-clause-not-building-correctly-with")]
        public void ShouldComparePropertiesAcrossInterfacesViaGenerics()
        {
            TestShouldComparePropertiesAcrossInterfacesViaGenerics<IFoo>();
        }

        static void TestShouldComparePropertiesAcrossInterfacesViaGenerics<TNode>() where TNode : IFoo
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<TNode, TNode, bool>> expression =
                (p1, p2) => p1.Bar == p2.Bar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(p1.Bar = p2.Bar)", result);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/99/throw-error-when-unary-expressions-are")]
        public void ThrowNotSupportedExceptionForMemberAccessExpression()
        {
            // Where<FooData>(n => n.Bar)

            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression =
                p1 => p1.SomeBool;

            Assert.Throws<NotSupportedException>(() =>
                CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v)));
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/99/throw-error-when-unary-expressions-are")]
        public void ThrowNotSupportedExceptionForUnaryNotExpression()
        {
            // Where<FooData>(n => !n.Bar)

            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression =
                p1 => !p1.SomeBool;

            Assert.Throws<NotSupportedException>(() =>
                CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v)));
        }

        [Test]
        public void GetsValueFromNestedProperty()
        {
            var comparison = new Nested {Foo = new Foo {Bar = BazField}};

            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.Bar == comparison.Foo.Bar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.Bar = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void GetsValueFromSuperNestedProperty()
        {
            var comparison = new SuperNested {Nested= new Nested {Foo = new Foo {Bar = BazField}}};

            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.Bar == comparison.Nested.Foo.Bar;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.Bar = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        static string CreateParameter(IDictionary<string, object> parameters, object paramValue)
        {
            var paramName = string.Format("p{0}", parameters.Count);
            parameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
        }
    }
}
