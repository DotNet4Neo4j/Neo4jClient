using System;
using System.Collections.Generic;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.Gremlin
{
    [TestFixture]
    public class FormatGremlinFilterTests
    {
        [Test]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseSensititiveFilters()
        {
            var filters = new Dictionary<string, object>();
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual(string.Empty, filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveFilterWithStringValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", "Bar" }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':'Bar']]", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveFilterWithIntValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", 123 }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':123]]", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveFilterWithLongValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", (long)123 }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':123]]", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveFilterWithLongMaxValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", long.MaxValue }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':9223372036854775807]]", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveFilterWithEnumValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", EnumForTesting.Bar }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':'Bar']]", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveFilterWithNullValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", null }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':null]]", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForMultipleCaseSensititiveFiltersWithStringValues()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", "Bar" },
                { "Baz", "Qak" }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("[['Foo':'Bar'],['Baz':'Qak']]", filterText);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "One or more of the supplied filters is of an unsupported type. Unsupported filters were: Foo of type System.ThreadStaticAttribute")]
        public void FormatGremlinFilterShouldThrowNotSupportedExceptionForCaseSensitiveFilterOfUnsupportedType()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", new ThreadStaticAttribute() }
            };
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseInsensitiveFilters()
        {
            var filters = new Dictionary<string, object>();
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual(string.Empty, filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveFilterWithStringValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", "Bar" }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveFilterWithIntValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", 123 }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' == 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveFilterWithLongValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", (long)123 }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' == 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveFilterWithLongMaxValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", long.MaxValue }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' == 9223372036854775807 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveFilterWithEnumValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", EnumForTesting.Bar }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForMultipleCaseInsensititiveFiltersWithStringValues()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", "Bar" },
                { "Baz", "Qak" }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') && it.'Baz'.equalsIgnoreCase('Qak') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveFilterWithNullValue()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", null }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' == null }", filterText);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "One or more of the supplied filters is of an unsupported type. Unsupported filters were: Foo of type System.ThreadStaticAttribute")]
        public void FormatGremlinFilterShouldThrowNotSupportedExceptionForCaseInsensitiveFilterOfUnsupportedType()
        {
            var filters = new Dictionary<string, object>
            {
                { "Foo", new ThreadStaticAttribute() }
            };
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
        }

        enum EnumForTesting
        {
            Bar
        }
    }
}
