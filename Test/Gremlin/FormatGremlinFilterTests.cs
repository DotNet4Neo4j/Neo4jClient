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
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual(string.Empty, filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo'.equals('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.Equal  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo' == 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEquakFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (long)123, ExpressionType = ExpressionType.Equal  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo' == 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithLongMaxValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.Equal  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo' == 9223372036854775807 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo'.equals('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithNullValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Equal  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo' == null }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForMultipleCaseSensititiveEqualFiltersWithStringValues()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                new Filter { PropertyName= "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo'.equals('Bar') && it.'Baz'.equals('Qak') }", filterText);
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
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseInsensitiveFilters()
        {
            var filters = new List<Filter>();
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual(string.Empty, filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.Equal },
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' == 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (long)123, ExpressionType = ExpressionType.Equal  },
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' == 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithLongMaxValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.Equal  },
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' == 9223372036854775807 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  },
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForMultipleCaseInsensititiveEqualFiltersWithStringValues()
        {
            var filters = new List<Filter>
            {
                new Filter {PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal },
                new Filter {PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal },
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.equalsIgnoreCase('Bar') && it.'Baz'.equalsIgnoreCase('Qak') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithNullValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Equal},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' == null }", filterText);
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
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual(expectedValue, filterText);
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
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual(expectedValue, filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveNotEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.NotEqual  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ !it.'Foo'.equals('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveNotEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.NotEqual  }
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ !it.'Foo'.equalsIgnoreCase('Bar') }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' > 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo' > 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' < 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo' < 123 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' > 9223372036854775807 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo' > 9223372036854775807 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo' < 9223372036854775807 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo' < 9223372036854775807 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.GreaterThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.compareToIgnoreCase('Bar') > 0 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.GreaterThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo'.compareTo('Bar') > 0 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.LessThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual("{ it.'Foo'.compareToIgnoreCase('Bar') < 0 }", filterText);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.LessThan},
            };
            var filterText = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal);
            Assert.AreEqual("{ it.'Foo'.compareTo('Bar') < 0 }", filterText);
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
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
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
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase);
        }

        enum EnumForTesting
        {
            Bar
        }
    }
}
