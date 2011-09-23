using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            var filters = new List<Filter>();
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(string.Empty, filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo'.equals('Bar') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' == 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEquakFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (long)123, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' == 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithLongMaxValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' == 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo'.equals('Bar') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithNullableEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (EnumForTesting?)EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo'.equals('Bar') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithNullValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' == null }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForMultipleCaseSensititiveEqualFiltersWithStringValues()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                new Filter { PropertyName= "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo'.equals('Bar') && it.'Baz'.equals('Qak') }", filter.FilterText);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "One or more of the supplied filters is of an unsupported type or expression. Unsupported filters were: Foo of type System.ThreadStaticAttribute, with expression Equal")]
        public void FormatGremlinFilterShouldThrowNotSupportedExceptionForCaseSensitiveEqualFilterOfUnsupportedType()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = new ThreadStaticAttribute(), ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseInsensitiveFilters()
        {
            var filters = new List<Filter>();
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(string.Empty, filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.Equal },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' == 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (long)123, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' == 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithLongMaxValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' == 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithNullableEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (EnumForTesting?)EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForMultipleCaseInsensititiveEqualFiltersWithStringValues()
        {
            var filters = new List<Filter>
            {
                new Filter {PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal },
                new Filter {PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') && it.'Baz'.equalsIgnoreCase('Qak') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithNullValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Equal},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' == null }", filter.FilterText);
        }

        [TestCase("Foo", "Bar", "{ !it.'Foo'.equalsIgnoreCase('Bar') }")]
        [TestCase("Foo", 9223372036854775807, "{ it.'Foo' != 9223372036854775807 }")]
        [TestCase("Foo", 0L, "{ it.'Foo' != 0 }")]
        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveNotEqualFilter(string propertyName, object valueToCompare, string expectedValue)
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= propertyName, Value = valueToCompare, ExpressionType = ExpressionType.NotEqual  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(expectedValue, filter.FilterText);
        }

        [TestCase("Foo", "Bar", "{ !it.'Foo'.equals('Bar') }")]
        [TestCase("Foo", 9223372036854775807, "{ it.'Foo' != 9223372036854775807 }")]
        [TestCase("Foo", 0L, "{ it.'Foo' != 0 }")]
        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseSensititiveNotEqualFilter(string propertyName, object valueToCompare, string expectedValue)
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= propertyName, Value = valueToCompare, ExpressionType = ExpressionType.NotEqual  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(expectedValue, filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveNotEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.NotEqual  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ !it.'Foo'.equals('Bar') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveNotEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.NotEqual  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ !it.'Foo'.equalsIgnoreCase('Bar') }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' > 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' > 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' < 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' < 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' > 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' > 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' < 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' < 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' >= 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' >= 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' <= 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' <= 123 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' >= 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' >= 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual("{ it.'Foo' <= 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual("{ it.'Foo' <= 9223372036854775807 }", filter.FilterText);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "One or more of the supplied filters is of an unsupported type or expression. Unsupported filters were: Foo of type System.ThreadStaticAttribute, with expression Equal")]
        public void FormatGremlinFilterShouldThrowNotSupportedExceptionForCaseInsensitiveEqualFilterOfUnsupportedType()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = new ThreadStaticAttribute(), ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
        }

        [Test]
        [ExpectedException(
            typeof(NotSupportedException),
            ExpectedMessage = "One or more of the supplied filters is of an unsupported type or expression. Unsupported filters were: Foo with null value and expression Divide")]
        public void FormatGremlinFilterShouldThrowNotSupportedExceptionForExpressionsThatAreNotRegisteredInFilterTypesToCompareNulls()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Divide },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>());
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
        }

        enum EnumForTesting
        {
            Bar
        }
    }
}
