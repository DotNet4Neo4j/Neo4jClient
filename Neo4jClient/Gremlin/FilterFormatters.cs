using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.Gremlin
{
    internal static class FilterFormatters
    {
        internal static string FormatGremlinFilter(IEnumerable<Filter> filters, StringComparison comparison)
        {
            filters = filters
                .Select(f =>
                {
                    if (f.Value != null && f.Value.GetType().IsEnum)
                        f.Value = f.Value.ToString();
                    return f;
                })
                .ToArray();

            if (filters.Any(f => !f.ExpressionType.HasValue))
                throw new ArgumentException("ExpressionType must not be null", "filters");

            IList<TypeFilter> typeFilterFormats = new List<TypeFilter>();
            string nullFilterExpression, filterSeparator, concatenatedFiltersFormat;

            typeFilterFormats.Add(new TypeFilter { Type =  typeof(int), FilterFormat = "it.'{0}' == {1}", ExpressionType = ExpressionType.Equal });
            typeFilterFormats.Add(new TypeFilter { Type =  typeof(long), FilterFormat = "it.'{0}' == {1}", ExpressionType = ExpressionType.Equal });
            typeFilterFormats.Add(new TypeFilter { Type = typeof(int), FilterFormat = "it.'{0}' != {1}", ExpressionType = ExpressionType.NotEqual  });
            typeFilterFormats.Add(new TypeFilter { Type = typeof(long), FilterFormat = "it.'{0}' != {1}", ExpressionType = ExpressionType.NotEqual });

            nullFilterExpression = "it.'{0}' == null";
            filterSeparator = " && ";
            concatenatedFiltersFormat = "{{ {0} }}";

            switch (comparison)
            {
                case StringComparison.Ordinal:
                        typeFilterFormats.Add(new TypeFilter { Type = typeof(string), FilterFormat = "it.'{0}'.equals('{1}')", ExpressionType = ExpressionType.Equal});
                        typeFilterFormats.Add(new TypeFilter { Type = typeof(string),FilterFormat = "!it.'{0}'.equals('{1}')", ExpressionType = ExpressionType.NotEqual  });
                    break;
                case StringComparison.OrdinalIgnoreCase:
                        typeFilterFormats.Add(new TypeFilter { Type = typeof(string),FilterFormat = "it.'{0}'.equalsIgnoreCase('{1}')", ExpressionType = ExpressionType.Equal  });
                        typeFilterFormats.Add(new TypeFilter { Type = typeof(string),FilterFormat = "!it.'{0}'.equalsIgnoreCase('{1}')", ExpressionType = ExpressionType.NotEqual  });
                    break;
                default:
                    throw new NotSupportedException(string.Format("Comparison mode {0} is not supported.", comparison));
            }

            var expandedFilters =
                from f in filters
                let filterValueType = f.Value == null ? null : f.Value.GetType()
                let supportedType = filterValueType == null || typeFilterFormats.Any(tf=> tf.Type == filterValueType)
                let filterFormat = supportedType
                    ? filterValueType == null ? nullFilterExpression : typeFilterFormats.Single(t => t.Type == filterValueType && t.ExpressionType == f.ExpressionType).FilterFormat
                    : null
                select new
                {
                    f.PropertyName,
                    f.Value,
                    f.ExpressionType,
                    SupportedType = supportedType,
                    ValueType = filterValueType,
                    Format = filterFormat
                };

            expandedFilters = expandedFilters.ToArray();

            var unsupportedFilters = expandedFilters
                .Where(f => !f.SupportedType)
                .Select(f => string.Format("{0} of type {1}", f.PropertyName, f.ValueType.FullName))
                .ToArray();
            if (unsupportedFilters.Any())
                throw new NotSupportedException(string.Format(
                    "One or more of the supplied filters is of an unsupported type. Unsupported filters were: {0}",
                    string.Join(", ", unsupportedFilters)));

            var formattedFilters = expandedFilters
                .Select(f => string.Format(f.Format, f.PropertyName, f.Value == null ? null : f.Value.ToString()))
                .ToArray();
            var concatenatedFilters = string.Join(filterSeparator, formattedFilters);
            if (!string.IsNullOrWhiteSpace(concatenatedFilters))
                concatenatedFilters = string.Format(concatenatedFiltersFormat, concatenatedFilters);
            return concatenatedFilters;
        }

        internal static void TranslateFilter<TNode>(Expression<Func<TNode, bool>> filter, IEnumerable<Filter> simpleFilters)
        {
            var binaryExpression = filter.Body as BinaryExpression;
            if (binaryExpression == null)
                throw new NotSupportedException("Only binary expressions are supported at this time.");
            if (binaryExpression.NodeType != ExpressionType.Equal)
                throw new NotSupportedException("Only equality expressions are supported at this time.");

            var key = ParseKeyFromExpression(binaryExpression.Left);
            var constantValue = ParseValueFromExpression(binaryExpression.Right);
            var convertedValue = key.PropertyType.IsEnum
                ? Enum.Parse(key.PropertyType, key.PropertyType.GetEnumName(constantValue))
                : constantValue;

            ((IList) simpleFilters).Add(
                new Filter
                    {
                        PropertyName = key.Name,
                        Value = convertedValue,
                        ExpressionType = binaryExpression.NodeType
                    });
        }

        static ExpressionKey ParseKeyFromExpression(Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null &&
                unaryExpression.NodeType == ExpressionType.Convert)
                expression = unaryExpression.Operand;

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null &&
                memberExpression.Member is PropertyInfo &&
                memberExpression.Member.MemberType == MemberTypes.Property)
                return new ExpressionKey
                {
                    Name = memberExpression.Member.Name,
                    PropertyType = ((PropertyInfo)memberExpression.Member).PropertyType
                };

            throw new NotSupportedException("Only property accessors are supported for the left-hand side of the expression at this time.");
        }

        static object ParseValueFromExpression(Expression expression)
        {
            var lambdaExpression = Expression.Lambda(expression);
            return lambdaExpression.Compile().DynamicInvoke();
        }

        class ExpressionKey
        {
            public string Name { get; set; }
            public Type PropertyType { get; set; }
        }
    }
}
