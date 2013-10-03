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
        public void EvaluateFalseWhenComparingMissingNullablePropertyToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void EvaluateTrueWhenComparingMissingNullablePropertyToNotConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar? <> {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/103/cypher-queries-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyGreaterThanConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar > 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! > {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/103/cypher-queries-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyGreaterThanOrEqualToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar >= 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! >= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/103/cypher-queries-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyLessThanConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar < 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! < {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/103/cypher-queries-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyLessThanOrEqualToConstantValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar <= 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! <= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void EvaluateTrueWhenComparingMissingNullablePropertyToNull()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == null;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar? is null)", result);
        }

        [Test]
        public void EvaluateFalseWhenComparingMissingNullablePropertyToNotNull()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != null;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar? is not null)", result);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyToLocalMemberValue()
        {
            var localObject = new {NoneCypherLocalProperty = 123};
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void EvaluateTrueWhenComparingMissingNullablePropertyToNotLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar? <> {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyGreaterThanLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar > localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! > {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyGreaterThanOrEqualToLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar >= localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! >= {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyLessThanLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar < localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! < {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/98/where-andwhere-include-nodes-with-missing")]
        public void EvaluateFalseWhenComparingMissingNullablePropertyLessThanOrEqualToLocalMemberValue()
        {
            var localObject = new { NoneCypherLocalProperty = 123 };
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar <= localObject.NoneCypherLocalProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, v => CreateParameter(parameters, v));

            Assert.AreEqual("(foo.NullableBar! <= {p0})", result);
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
