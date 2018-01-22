using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Gremlin
{
    
    public class FormatGremlinFilterTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void FormatGremlinFilterShouldSupportGuidTypeInEqualsExpression()
        {
            const string guidString = "1a4e451c-aa87-4388-9b53-5d00b05ac728";
            var guidValue = Guid.Parse(guidString);

            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = guidValue, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(guidString, filter.FilterParameters["p1"].ToString());
        }

        [Fact]
        public void FormatGremlinFilterShouldSupportGuidTypeInNotEqualsExpression()
        {
            const string guidString = "1a4e451c-aa87-4388-9b53-5d00b05ac728";
            var guidValue = Guid.Parse(guidString);

            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = guidValue, ExpressionType = ExpressionType.NotEqual  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(guidString, filter.FilterParameters["p1"].ToString());
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseSensititiveFilters()
        {
            var filters = new List<Filter>();
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(string.Empty, filter.FilterText);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0].equals(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEquakFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (long)123, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123L, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithLongMaxValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0].equals(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithNullableEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (EnumForTesting?)EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0].equals(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveEqualFilterWithNullValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] == null }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForMultipleCaseSensititiveEqualFiltersWithStringValues()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
                new Filter { PropertyName= "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0].equals(p1) && it[p2].equals(p3) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
            Assert.Equal("Baz", filter.FilterParameters["p2"]);
            Assert.Equal("Qak", filter.FilterParameters["p3"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldThrowNotSupportedExceptionForCaseSensitiveEqualFilterOfUnsupportedType()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = new ThreadStaticAttribute(), ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery));
            Assert.Equal("One or more of the supplied filters is of an unsupported type or expression. Unsupported filters were: Foo of type System.ThreadStaticAttribute, with expression Equal", ex.Message);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnEmptyStringForNoCaseInsensitiveFilters()
        {
            var filters = new List<Filter>();
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(string.Empty, filter.FilterText);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.Equal },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (long)123, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123L, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithLongMaxValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithNullableEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = (EnumForTesting?)EnumForTesting.Bar, ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForMultipleCaseInsensititiveEqualFiltersWithStringValues()
        {
            var filters = new List<Filter>
            {
                new Filter {PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.Equal },
                new Filter {PropertyName = "Baz", Value = "Qak", ExpressionType = ExpressionType.Equal },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0].equalsIgnoreCase(p1) && it[p2].equalsIgnoreCase(p3) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
            Assert.Equal("Baz", filter.FilterParameters["p2"]);
            Assert.Equal("Qak", filter.FilterParameters["p3"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveEqualFilterWithNullValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Equal},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] == null }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveNotEqualFilterStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter {PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.NotEqual}
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ !it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveNotEqualFilterMaxLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName = "Foo", Value = 9223372036854775807, ExpressionType = ExpressionType.NotEqual },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseInsensititiveNotEqualFilterZeroLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName = "Foo", Value = 0L, ExpressionType = ExpressionType.NotEqual  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(0L, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseSensititiveNotEqualFilterStringValue()
        {
            var filters = new List<Filter>
            {
                new Filter {PropertyName = "Foo", Value = "Bar", ExpressionType = ExpressionType.NotEqual}
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ !it[p0].equals(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseSensititiveNotEqualFilterMaxLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName = "Foo", Value = 9223372036854775807, ExpressionType = ExpressionType.NotEqual },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIteratorSyntaxForSingleCaseSensititiveNotEqualFilterZeroLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName = "Foo", Value = 0L, ExpressionType = ExpressionType.NotEqual  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] != p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(0L, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveNotEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.NotEqual  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ !it[p0].equals(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveNotEqualFilterWithEnumValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = EnumForTesting.Bar, ExpressionType = ExpressionType.NotEqual  }
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ !it[p0].equalsIgnoreCase(p1) }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal("Bar", filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] > p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] > p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] < p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] < p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] > p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] > p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] < p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThan},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] < p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] >= p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] >= p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] <= p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanOrEqualFilterWithIntValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = 123, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] <= p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(123, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveGreaterThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] >= p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveGreaterThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.GreaterThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] >= p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseInsensititiveLessThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery);
            Assert.Equal(".filter{ it[p0] <= p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleCaseSensititiveLessThanOrEqualFilterWithLongValue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = long.MaxValue, ExpressionType = ExpressionType.LessThanOrEqual},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] <= p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(9223372036854775807, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldThrowNotSupportedExceptionForCaseInsensitiveEqualFilterOfUnsupportedType()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = new ThreadStaticAttribute(), ExpressionType = ExpressionType.Equal  },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery));
            Assert.Equal("One or more of the supplied filters is of an unsupported type or expression. Unsupported filters were: Foo of type System.ThreadStaticAttribute, with expression Equal", ex.Message);
        }

        [Fact]
        public void FormatGremlinFilterShouldThrowNotSupportedExceptionForExpressionsThatAreNotRegisteredInFilterTypesToCompareNulls()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = null, ExpressionType = ExpressionType.Divide },
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var ex = Assert.Throws<NotSupportedException>(() => FilterFormatters.FormatGremlinFilter(filters, StringComparison.OrdinalIgnoreCase, baseQuery));
            Assert.Equal("One or more of the supplied filters is of an unsupported type or expression. Unsupported filters were: Foo with null value and expression Divide", ex.Message);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleEqualFilterWithBoolValueTrue()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = true, ExpressionType = ExpressionType.Equal},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(true, filter.FilterParameters["p1"]);
        }

        [Fact]
        public void FormatGremlinFilterShouldReturnIndexerSyntaxForSingleEqualFilterWithBoolValueFalse()
        {
            var filters = new List<Filter>
            {
                new Filter { PropertyName= "Foo", Value = false, ExpressionType = ExpressionType.Equal},
            };
            var baseQuery = new GremlinQuery(null, null, new Dictionary<string, object>(), new List<string>());
            var filter = FilterFormatters.FormatGremlinFilter(filters, StringComparison.Ordinal, baseQuery);
            Assert.Equal(".filter{ it[p0] == p1 }", filter.FilterText);
            Assert.Equal("Foo", filter.FilterParameters["p0"]);
            Assert.Equal(false, filter.FilterParameters["p1"]);
        }

        enum EnumForTesting
        {
            Bar
        }
    }
}
