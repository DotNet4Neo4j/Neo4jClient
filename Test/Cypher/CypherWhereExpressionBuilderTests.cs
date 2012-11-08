using NUnit.Framework;
using Neo4jClient.Cypher;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;

namespace Neo4jClient.Test.Cypher
{
    public class CypherWhereExpressionBuilderTests
    {
        class Foo
        {
            public int Bar { get; set; }
            public int? NullableBar { get; set; }
        }

        // This must be a public static field, that's not a constant
        public static int BazField = 123;

        // This must be a public static property
        public static int BazProperty
        {
            get { return 456; }
        }

        [Test]
        public void AccessStaticField()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.Bar == BazField;

            var result = CypherWhereExpressionBuilder.BuildText(expression, parameters);

            Assert.AreEqual("(foo.Bar = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void AccessStaticProperty()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.Bar == BazProperty;

            var result = CypherWhereExpressionBuilder.BuildText(expression, parameters);

            Assert.AreEqual("(foo.Bar = {p0})", result);
            Assert.AreEqual(456, parameters["p0"]);
        }

        [Test]
        public void EvaluateFalseWhenComparingMissingNullablePropertyToValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, parameters);

            Assert.AreEqual("(foo.NullableBar! = {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void EvaluateTrueWhenComparingMissingNullablePropertyToNotValue()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != 123;

            var result = CypherWhereExpressionBuilder.BuildText(expression, parameters);

            Assert.AreEqual("(foo.NullableBar? <> {p0})", result);
            Assert.AreEqual(123, parameters["p0"]);
        }

        [Test]
        public void EvaluateTrueWhenComparingMissingNullablePropertyToNull()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar == null;

            var result = CypherWhereExpressionBuilder.BuildText(expression, parameters);

            Assert.AreEqual("(foo.NullableBar? is null)", result);
        }

        [Test]
        public void EvaluateFalseWhenComparingMissingNullablePropertyToNotNull()
        {
            var parameters = new Dictionary<string, object>();
            Expression<Func<Foo, bool>> expression = foo => foo.NullableBar != null;

            var result = CypherWhereExpressionBuilder.BuildText(expression, parameters);

            Assert.AreEqual("(foo.NullableBar? is not null)", result);
        }
    }
}
