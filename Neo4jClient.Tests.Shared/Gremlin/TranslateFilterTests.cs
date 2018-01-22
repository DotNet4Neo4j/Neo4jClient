using System;
using System.Globalization;
using System.Linq;
using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class TranslateFilterTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantStringExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == "abc" // This must be a constant - do not refactor this line
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal("abc", filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantIntExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithIntegers>(
                    f => f.Prop1 == 123 // This must be a constant - do not refactor this line
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal(123, filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantLongExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithLongs>(
                    f => f.Prop1 == 123 // This must be a constant - do not refactor this line
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal((long)123, filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantLongMaxExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithLongs>(
                    f => f.Prop1 == long.MaxValue // This must be a constant - do not refactor this line
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal(long.MaxValue, filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalStringExpression()
        {
            var prop1Value = new string(new[] { 'a', 'b', 'c' }); // This must be a local - do not refactor this to a constant
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == prop1Value
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal("abc", filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalIntExpression()
        {
            var prop1Value = int.Parse("123"); // This must be a local - do not refactor this to a constant
            var filters = FilterFormatters
                .TranslateFilter<NodeWithIntegers>(
                    f => f.Prop1 == prop1Value
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal(123, filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalLongExpression()
        {
            var prop1Value = long.Parse("123"); // This must be a local - do not refactor this to a constant
            var filters = FilterFormatters
                .TranslateFilter<NodeWithLongs>(f => f.Prop1 == prop1Value)
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal(123L, filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalLongMaxExpression()
        {
            var prop1Value = long.Parse(long.MaxValue.ToString(CultureInfo.InvariantCulture)); // This must be a local - do not refactor this to a constant
            var filters = FilterFormatters
                .TranslateFilter<NodeWithLongs>(
                    f => f.Prop1 == prop1Value
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal(long.MaxValue, filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantEnumExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<NodeWithEnums>(f => f.Prop1 == EnumForTesting.Foo)
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal(EnumForTesting.Foo, filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAnotherStringPropertyExpression()
        {
            var bar = new Bar { Prop1 = "def" };
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == bar.Prop1 // This must be a property get - do not refactor this line
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal("def", filters.FirstOrDefault().Value);
        }

        [Fact]
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
            Assert.Equal(3, filters.Count());
            Assert.Equal("Prop1", filters[0].PropertyName);
            Assert.Equal("def", filters[0].Value);
            Assert.Equal("Prop2", filters[1].PropertyName);
            Assert.Equal("ghi", filters[1].Value);
            Assert.Equal("Prop3", filters[2].PropertyName);
            Assert.Equal("jkl", filters[2].Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAStringFunctionExpression()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(
                    f => f.Prop1 == string.Format("{0}.{1}", "abc", "def").ToUpperInvariant() // This must be a method call - do not refactor this line
                )
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal("ABC.DEF", filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveSinglePropertyEqualsNull()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null)
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.Equal(null, filters.FirstOrDefault().Value);
        }

        [Fact]
        public void TranslateFilterShouldResolvePropertiesEqualBoolean()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop4 == true)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop4", filters[0].PropertyName);
            Assert.Equal(true, filters[0].Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveBooleanPropertyToDefaultToCompareToTrue()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop4)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop4", filters[0].PropertyName);
            Assert.Equal(true, filters[0].Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveBooleanPropertyToDefaultToCompareToFalse()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => !f.Prop4)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.Equal(1, filters.Count());
            Assert.Equal("Prop4", filters[0].PropertyName);
            Assert.Equal(false, filters[0].Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveTwoPropertiesEqualNullWithBinaryAnd()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null & f.Prop2 == null)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.Equal(2, filters.Count());
            Assert.Equal("Prop1", filters[0].PropertyName);
            Assert.Equal(null, filters[0].Value);
            Assert.Equal("Prop2", filters[1].PropertyName);
            Assert.Equal(null, filters[1].Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveTwoPropertiesEqualNullWithBinaryAndAlso()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null && f.Prop2 == null)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.Equal(2, filters.Count());
            Assert.Equal("Prop1", filters[0].PropertyName);
            Assert.Equal(null, filters[0].Value);
            Assert.Equal("Prop2", filters[1].PropertyName);
            Assert.Equal(null, filters[1].Value);
        }

        [Fact]
        public void TranslateFilterShouldResolveThreePropertiesEqualNullWithBinaryAndAlso()
        {
            var filters = FilterFormatters
                .TranslateFilter<Foo>(f => f.Prop1 == null && f.Prop2 == null && f.Prop3 == null)
                .OrderBy(f => f.PropertyName)
                .ToArray();
            Assert.Equal(3, filters.Count());
            Assert.Equal("Prop1", filters[0].PropertyName);
            Assert.Equal(null, filters[0].Value);
            Assert.Equal("Prop2", filters[1].PropertyName);
            Assert.Equal(null, filters[1].Value);
            Assert.Equal("Prop3", filters[2].PropertyName);
            Assert.Equal(null, filters[2].Value);
        }

        [Fact]
        public void TranslateFilterShouldThrowExceptionIfOuterExpressionIsAndAlsoAndInnerLeftExpressionIsNotABinaryExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.TranslateFilter<Foo>(f => testVariable && f.Prop2 == null));


            Assert.True(ex.Message.StartsWith("This expression is not a binary expression:"));
        }

        [Fact]
        public void TranslateFilterShouldThrowExceptionIfOuterExpressionIsAndAlsoAndInnerRightExpressionIsNotABinaryExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null && testVariable));

            Assert.True(ex.Message.StartsWith("This expression is not a binary expression:"));
        }

        [Fact]
        public void TranslateFilterShouldThrowExceptionForOrExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null | testVariable));

            Assert.True(ex.Message.StartsWith("Oprerator Or is not yet supported."));
        }

        [Fact]
        public void TranslateFilterShouldThrowExceptionForOrElseExpression()
        {
            var testVariable = bool.Parse(bool.TrueString);
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.TranslateFilter<Foo>(f => f.Prop2 == null || testVariable));

            Assert.True(ex.Message.StartsWith("Oprerator OrElse is not yet supported."));
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
