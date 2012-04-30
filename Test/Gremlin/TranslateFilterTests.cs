using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class TranslateFilterTests
    {
        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantStringExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == "abc" // This must be a constant - do not refactor this line
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("abc", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantIntExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithIntegers>(
                    f => f.Prop1 == 123 // This must be a constant - do not refactor this line
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(123, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantLongExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithLongs>(
                    f => f.Prop1 == 123 // This must be a constant - do not refactor this line
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual((long)123, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantLongMaxExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithLongs>(
                    f => f.Prop1 == long.MaxValue // This must be a constant - do not refactor this line
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(long.MaxValue, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalStringExpression()
        {
            var prop1Value = new string(new[] { 'a', 'b', 'c' }); // This must be a local - do not refactor this to a constant
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == prop1Value
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("abc", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalIntExpression()
        {
            var prop1Value = int.Parse("123"); // This must be a local - do not refactor this to a constant
            var filters = FilterFormatters
                .TranslateFilter<NodeWithIntegers>(
                    f => f.Prop1 == prop1Value
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(123, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalLongExpression()
        {
            var prop1Value = long.Parse("123"); // This must be a local - do not refactor this to a constant
            var filters = FilterFormatters
                .TranslateFilter<NodeWithLongs>(f => f.Prop1 == prop1Value)
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(123, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalLongMaxExpression()
        {
            var prop1Value = long.Parse(long.MaxValue.ToString(CultureInfo.InvariantCulture)); // This must be a local - do not refactor this to a constant
            var filters = FilterFormatters
                .TranslateFilter<NodeWithLongs>(
                    f => f.Prop1 == prop1Value
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(long.MaxValue, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantEnumExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithEnums>(f => f.Prop1 == EnumForTesting.Foo)
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(EnumForTesting.Foo, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAnotherStringPropertyExpression()
        {
            var bar = new Bar { Prop1 = "def" };
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == bar.Prop1 // This must be a property get - do not refactor this line
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("def", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAStringFunctionExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == string.Format("{0}.{1}", "abc", "def").ToUpperInvariant() // This must be a method call - do not refactor this line
                )
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("ABC.DEF", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsNull()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null)
                .ToArray();
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(null, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveMultiplePropertiesEqualNullWithBinaryAnd()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null & f.Prop2 == null)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.AreEqual(2, filters.Count());
            Assert.AreEqual("Prop1", filters[0].PropertyName);
            Assert.AreEqual(null, filters[0].Value);
            Assert.AreEqual("Prop2", filters[1].PropertyName);
            Assert.AreEqual(null, filters[1].Value);
        }

        [Test]
        public void TranslateFilterShouldResolveMultiplePropertiesEqualNullWithBinaryAndAlso()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null && f.Prop2 == null)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.AreEqual(2, filters.Count());
            Assert.AreEqual("Prop1", filters[0].PropertyName);
            Assert.AreEqual(null, filters[0].Value);
            Assert.AreEqual("Prop2", filters[1].PropertyName);
            Assert.AreEqual(null, filters[1].Value);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "This expression is not a binary expression:",
            MatchType = MessageMatch.StartsWith)]
        public void TranslateFilterShouldThrowExceptionIfOuterExpressionIsAndAlsoAndInnerLeftExpressionIsNotABinaryExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            FilterFormatters.TranslateFilter<Foo>(f => testVariable && f.Prop2 == null);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "This expression is not a binary expression:",
            MatchType = MessageMatch.StartsWith)]
        public void TranslateFilterShouldThrowExceptionIfOuterExpressionIsAndAlsoAndInnerRightExpressionIsNotABinaryExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null && testVariable);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "Oprerator Or is not yet supported.",
            MatchType = MessageMatch.StartsWith)]
        public void TranslateFilterShouldThrowExceptionForOrExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null | testVariable);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "Oprerator OrElse is not yet supported.",
            MatchType = MessageMatch.StartsWith)]
        public void TranslateFilterShouldThrowExceptionForOrElseExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null || testVariable);
        }

        public class Foo
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        public class Bar
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        public class NodeWithIntegers
        {
            public int Prop1 { get; set; }
            public int Prop2 { get; set; }
        }

        public class NodeWithLongs
        {
            public long Prop1 { get; set; }
            public long Prop2 { get; set; }
        }

        public enum EnumForTesting
        {
            Foo,
            Bar,
            Baz
        }

        public class NodeWithEnums
        {
            public EnumForTesting Prop1 { get; set; }
        }
    }
}
