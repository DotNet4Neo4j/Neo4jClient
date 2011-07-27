using System.Collections.Generic;
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
            var filters = new List<Filter>();
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == "abc", // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("abc", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantIntExpression()
        {
            var filters = new List<Filter>();
            FilterFormatters.TranslateFilter<NodeWithIntegers>(
                f => f.Prop1 == 123, // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(123, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantLongExpression()
        {
            var filters = new List<Filter>();
            FilterFormatters.TranslateFilter<NodeWithLongs>(
                f => f.Prop1 == 123, // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual((long)123, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantLongMaxExpression()
        {
            var filters = new List<Filter>();
            FilterFormatters.TranslateFilter<NodeWithLongs>(
                f => f.Prop1 == long.MaxValue, // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(long.MaxValue, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalStringExpression()
        {
            var filters = new List<Filter>();
            var prop1Value = new string(new[] { 'a', 'b', 'c' }); // This must be a local - do not refactor this to a constant
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("abc", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalIntExpression()
        {
            var filters = new List<Filter>();
            var prop1Value = int.Parse("123"); // This must be a local - do not refactor this to a constant
            FilterFormatters.TranslateFilter<NodeWithIntegers>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(123, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalLongExpression()
        {
            var filters = new List<Filter>();
            var prop1Value = long.Parse("123"); // This must be a local - do not refactor this to a constant
            FilterFormatters.TranslateFilter<NodeWithLongs>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(123, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalLongMaxExpression()
        {
            var filters = new List<Filter>();
            var prop1Value = long.Parse(long.MaxValue.ToString()); // This must be a local - do not refactor this to a constant
            FilterFormatters.TranslateFilter<NodeWithLongs>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(long.MaxValue, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantEnumExpression()
        {
            var filters = new List<Filter>();
            FilterFormatters.TranslateFilter<NodeWithEnums>(
                f => f.Prop1 == EnumForTesting.Foo,
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(EnumForTesting.Foo, filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAnotherStringPropertyExpression()
        {
            var filters = new List<Filter>();
            var bar = new Bar { Prop1 = "def" };
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == bar.Prop1, // This must be a method call - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("def", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAStringFunctionExpression()
        {
            var filters = new List<Filter>();
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == string.Format("{0}.{1}", "abc", "def").ToUpperInvariant(), // This must be a method call - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual("ABC.DEF", filters.FirstOrDefault().Value);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsNull()
        {
            var filters = new List<Filter>();
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == null,
                filters
            );
            Assert.AreEqual("Prop1", filters.FirstOrDefault().PropertyName);
            Assert.AreEqual(null, filters.FirstOrDefault().Value);
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
