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
        public void FormatGremlinFilterShouldSupportGuidTypeInEqualsExpression()
        {
            var guidString = "1a4e451c-aa87-4388-9b53-5d00b05ac728";
            var guidValue = Guid.Parse(guidString);
            
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = guidValue, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(guidString, filter.FilterParameters["p1"]);
        }
        
        [Test]
        public void FormatGremlinFilterShouldSupportGuidTypeInNotEqualsExpression()
        {
            var guidString = "1a4e451c-aa87-4388-9b53-5d00b05ac728";
            var guidValue = Guid.Parse(guidString);
            
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = guidValue, ExpressionType = ExpressionType.NotEqual  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(guidString, filter.FilterParameters["p1"]);
        }
        
        [Test]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseSensititiveFilters()
        {
            var filters = new List<Filter>();
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
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
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0].equals(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEquakFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (long)123, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithLongMaxValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0].equals(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithNullableEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (EnumForTesting?)EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0].equals(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithNullValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == null }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForMultipleCaseSensititiveEqualFiltersWithStringValues()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                new Filter { PropertyName= "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0].equals(p1) && it[p2].equals(p3) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
            Assert.AreEqual("Baz", filter.FilterParameters["p2"]);
            Assert.AreEqual("Qak", filter.FilterParameters["p3"]);
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
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseInsensitiveFilters()
        {
            var filters = new List<Filter>();
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
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
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.Equal },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (long)123, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithLongMaxValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithNullableEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (EnumForTesting?)EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForMultipleCaseInsensititiveEqualFiltersWithStringValues()
        {
            var filters = new List<Filter>
            {
                new Filter {PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal },
                new Filter {PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0].equalsIgnoreCase(p1) && it[p2].equalsIgnoreCase(p3) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
            Assert.AreEqual("Baz", filter.FilterParameters["p2"]);
            Assert.AreEqual("Qak", filter.FilterParameters["p3"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithNullValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Equal},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == null }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveNotEqualFilterStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter {PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.NotEqual}
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ !it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveNotEqualFilterMaxLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName = "Foo", Value = 9223372036854775807, ExpressionType = ExpressionType.NotEqual },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveNotEqualFilterZeroLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName = "Foo", Value = 0L, ExpressionType = ExpressionType.NotEqual  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(0, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseSensititiveNotEqualFilterStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter {PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.NotEqual}
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ !it[p0].equals(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseSensititiveNotEqualFilterMaxLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName = "Foo", Value = 9223372036854775807, ExpressionType = ExpressionType.NotEqual },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseSensititiveNotEqualFilterZeroLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName = "Foo", Value = 0L, ExpressionType = ExpressionType.NotEqual  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(0, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveNotEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.NotEqual  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ !it[p0].equals(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveNotEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.NotEqual  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ !it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual("Bar", filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] > p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] > p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] < p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] < p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] > p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] > p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] < p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] < p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] >= p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] >= p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] <= p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] <= p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(123, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] >= p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] >= p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.AreEqual(".filter{ it[p0] <= p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] <= p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(9223372036854775807, filter.FilterParameters["p1"]);
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
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
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
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleEqualFilterWithBoolValueTrue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = true, ExpressionType = ExpressionType.Equal},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(true, filter.FilterParameters["p1"]);
        }

        [Test]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleEqualFilterWithBoolValueFalse()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = false, ExpressionType = ExpressionType.Equal},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.AreEqual(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.AreEqual("Foo", filter.FilterParameters["p0"]);
            Assert.AreEqual(false, filter.FilterParameters["p1"]);
        }

        enum EnumForTesting
        {
            Bar
        }
    }
}
