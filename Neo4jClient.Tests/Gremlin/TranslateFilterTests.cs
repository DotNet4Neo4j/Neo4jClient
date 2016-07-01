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
            Assert.AreEqual(1, filters.Count());
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
            Assert.AreEqual(1, filters.Count());
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
            Assert.AreEqual(1, filters.Count());
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
            Assert.AreEqual(1, filters.Count());
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
            Assert.AreEqual(1, filters.Count());
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
            Assert.AreEqual(1, filters.Count());
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
            Assert.AreEqual(1, filters.Count());
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
            Assert.AreEqual(1, filters.Count());
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(long.MaxValue, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantEnumExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithEnums>(f => f.Prop1 == EnumForTesting.Foo)
                .ToArray();
            Assert.AreEqual(1, filters.Count());
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
            Assert.AreEqual(1, filters.Count());
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("def", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveThreePropertiesEqualOtherStringPropertiesInBooleanAndAlsoChain()
        {
            var bar = new Bar { Prop1 = "def", Prop2 = "ghi", Prop3 = "jkl" };
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    // These must be property gets - do not refactor this line
                    f => f.Prop1 == bar.Prop1 && f.Prop2 == bar.Prop2 && f.Prop3 == bar.Prop3
                )
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.AreEqual(3, filters.Count());
            Assert.AreEqual("Prop1", filters[0].PropertyName);
            Assert.AreEqual("def", filters[0].Value);
            Assert.AreEqual("Prop2", filters[1].PropertyName);
            Assert.AreEqual("ghi", filters[1].Value);
            Assert.AreEqual("Prop3", filters[2].PropertyName);
            Assert.AreEqual("jkl", filters[2].Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAStringFunctionExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == string.Format("{0}.{1}", "abc", "def").ToUpperInvariant() // This must be a method call - do not refactor this line
                )
                .ToArray();
            Assert.AreEqual(1, filters.Count());
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("ABC.DEF", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsNull()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null)
                .ToArray();
            Assert.AreEqual(1, filters.Count());
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(null, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolvePropertiesEqualBoolean()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop4 == true)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.AreEqual(1, filters.Count());
            Assert.AreEqual("Prop4", filters[0].PropertyName);
            Assert.AreEqual(true, filters[0].Value);
        }

        [Test]
        public void TranslateFilterShouldResolveBooleanPropertyToDefaultToCompareToTrue()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop4)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.AreEqual(1, filters.Count());
            Assert.AreEqual("Prop4", filters[0].PropertyName);
            Assert.AreEqual(true, filters[0].Value);
        }

        [Test]
        public void TranslateFilterShouldResolveBooleanPropertyToDefaultToCompareToFalse()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => !f.Prop4)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.AreEqual(1, filters.Count());
            Assert.AreEqual("Prop4", filters[0].PropertyName);
            Assert.AreEqual(false, filters[0].Value);
        }

        [Test]
        public void TranslateFilterShouldResolveTwoPropertiesEqualNullWithBinaryAnd()
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
        public void TranslateFilterShouldResolveTwoPropertiesEqualNullWithBinaryAndAlso()
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
        public void TranslateFilterShouldResolveThreePropertiesEqualNullWithBinaryAndAlso()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null && f.Prop2 == null && f.Prop3 == null)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.AreEqual(3, filters.Count());
            Assert.AreEqual("Prop1", filters[0].PropertyName);
            Assert.AreEqual(null, filters[0].Value);
            Assert.AreEqual("Prop2", filters[1].PropertyName);
            Assert.AreEqual(null, filters[1].Value);
            Assert.AreEqual("Prop3", filters[2].PropertyName);
            Assert.AreEqual(null, filters[2].Value);
        }

        [Test]
        public void TranslateFilterShouldThrowExceptionIfOuterExpressionIsAndAlsoAndInnerLeftExpressionIsNotABinaryExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.TranslateFilter<Foo>(f => testVariable && f.Prop2 == null));


            Assert.IsTrue(ex.Message.StartsWith("This expression is not a binary expression:"));
        }

        [Test]
        public void TranslateFilterShouldThrowExceptionIfOuterExpressionIsAndAlsoAndInnerRightExpressionIsNotABinaryExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null && testVariable));

            Assert.IsTrue(ex.Message.StartsWith("This expression is not a binary expression:"));
        }

        [Test]
        public void TranslateFilterShouldThrowExceptionForOrExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null | testVariable));

            Assert.IsTrue(ex.Message.StartsWith("Oprerator Or is not yet supported."));
        }

        [Test]
        public void TranslateFilterShouldThrowExceptionForOrElseExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null || testVariable));

            Assert.IsTrue(ex.Message.StartsWith("Oprerator OrElse is not yet supported."));
        }

        public class Foo
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            public string Prop3 { get; set; }
            public bool Prop4 { get; set; }
        }

        public class Bar
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            public string Prop3 { get; set; }
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
