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
            var filters = new Dictionary<string, object>();
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == "abc", // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual("abc", filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantIntExpression()
        {
            var filters = new Dictionary<string, object>();
            FilterFormatters.TranslateFilter<NodeWithIntegers>(
                f => f.Prop1 == 123, // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual(123, filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantLongExpression()
        {
            var filters = new Dictionary<string, object>();
            FilterFormatters.TranslateFilter<NodeWithLongs>(
                f => f.Prop1 == 123, // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual((long)123, filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsConstantLongMaxExpression()
        {
            var filters = new Dictionary<string, object>();
            FilterFormatters.TranslateFilter<NodeWithLongs>(
                f => f.Prop1 == long.MaxValue, // This must be a constant - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual(long.MaxValue, filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalStringExpression()
        {
            var filters = new Dictionary<string, object>();
            var prop1Value = new string(new[] { 'a', 'b', 'c' }); // This must be a local - do not refactor this to a constant
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual("abc", filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalIntExpression()
        {
            var filters = new Dictionary<string, object>();
            var prop1Value = int.Parse("123"); // This must be a local - do not refactor this to a constant
            FilterFormatters.TranslateFilter<NodeWithIntegers>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual(123, filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalLongExpression()
        {
            var filters = new Dictionary<string, object>();
            var prop1Value = long.Parse("123"); // This must be a local - do not refactor this to a constant
            FilterFormatters.TranslateFilter<NodeWithLongs>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual(123, filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsLocalLongMaxExpression()
        {
            var filters = new Dictionary<string, object>();
            var prop1Value = long.Parse(long.MaxValue.ToString()); // This must be a local - do not refactor this to a constant
            FilterFormatters.TranslateFilter<NodeWithLongs>(
                f => f.Prop1 == prop1Value,
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual(long.MaxValue, filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAnotherStringPropertyExpression()
        {
            var filters = new Dictionary<string, object>();
            var bar = new Bar { Prop1 = "def" };
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == bar.Prop1, // This must be a method call - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual("def", filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsAStringFunctionExpression()
        {
            var filters = new Dictionary<string, object>();
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == string.Format("{0}.{1}", "abc", "def").ToUpperInvariant(), // This must be a method call - do not refactor this line
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual("ABC.DEF", filters["Prop1"]);
        }

        [Test]
        public void TranslateFilterShouldResolveSinglePropertyEqualsNull()
        {
            var filters = new Dictionary<string, object>();
            FilterFormatters.TranslateFilter<Foo>(
                f => f.Prop1 == null,
                filters
            );
            Assert.AreEqual("Prop1", filters.Keys.Single());
            Assert.AreEqual(null, filters["Prop1"]);
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
    }
}
