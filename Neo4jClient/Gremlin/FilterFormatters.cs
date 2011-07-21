using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.Gremlin
{
    internal static class FilterFormatters
    {
        internal static string FormatGremlinFilter(IDictionary<string, object> filters, StringComparison comparison)
        {
            IDictionary<Type, string> typeFilterFormats;
            string nullFilterExpression, filterSeparator, concatenatedFiltersFormat;
            switch (comparison)
            {
                case StringComparison.Ordinal:
                    typeFilterFormats = new Dictionary<Type, string>
                    {
                        { typeof(string), "['{0}':'{1}']" },
                        { typeof(int), "['{0}':{1}]" },
                        { typeof(long), "['{0}':{1}]" },
                    };
                    nullFilterExpression = "['{0}':null]";
                    filterSeparator = ",";
                    concatenatedFiltersFormat = "[{0}]";
                    break;
                case StringComparison.OrdinalIgnoreCase:
                    typeFilterFormats = new Dictionary<Type, string>
                    {
                        { typeof(string), "it.'{0}'.equalsIgnoreCase('{1}')" },
                        { typeof(int), "it.'{0}' == {1}" },
                        { typeof(long), "it.'{0}' == {1}" },
                    };
                    nullFilterExpression = "it.'{0}' == null";
                    filterSeparator = " && ";
                    concatenatedFiltersFormat = "{{ {0} }}";
                    break;
                default:
                    throw new NotSupportedException(string.Format("Comparison mode {0} is not supported.", comparison));
            }

            var expandedFilters =
                from f in filters
                let filterValueType = f.Value == null ? null : f.Value.GetType()
                let supportedType = filterValueType == null || typeFilterFormats.ContainsKey(filterValueType)
                let filterFormat = supportedType
                    ? filterValueType == null ? nullFilterExpression : typeFilterFormats[filterValueType]
                    : null
                select new
                {
                    f.Key,
                    f.Value,
                    SupportedType = supportedType,
                    ValueType = filterValueType,
                    Format = filterFormat
                };

            expandedFilters = expandedFilters.ToArray();

            var unsupportedFilters = expandedFilters
                .Where(f => !f.SupportedType)
                .Select(f => string.Format("{0} of type {1}", f.Key, f.ValueType.FullName))
                .ToArray();
            if (unsupportedFilters.Any())
                throw new NotSupportedException(string.Format(
                    "One or more of the supplied filters is of an unsupported type. Unsupported filters were: {0}",
                    string.Join(", ", unsupportedFilters)));

            var formattedFilters = expandedFilters
                .Select(f => string.Format(f.Format, f.Key, f.Value == null ? null : f.Value.ToString()))
                .ToArray();
            var concatenatedFilters = string.Join(filterSeparator, formattedFilters);
            if (!string.IsNullOrWhiteSpace(concatenatedFilters))
                concatenatedFilters = string.Format(concatenatedFiltersFormat, concatenatedFilters);
            return concatenatedFilters;
        }

        internal static void TranslateFilter<TNode>(Expression<Func<TNode, bool>> filter, IDictionary<string, object> simpleFilters)
        {
            var binaryExpression = filter.Body as BinaryExpression;
            if (binaryExpression == null)
                throw new NotSupportedException("Only binary expressions are supported at this time.");
            if (binaryExpression.NodeType != ExpressionType.Equal)
                throw new NotSupportedException("Only equality expressions are supported at this time.");

            var propertyName = ParseKeyFromExpression(binaryExpression.Left);
            var constantValue = ParseValueFromExpression(binaryExpression.Right);

            simpleFilters.Add(propertyName, constantValue);
        }

        static string ParseKeyFromExpression(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null &&
                memberExpression.Member.MemberType == MemberTypes.Property)
                return memberExpression.Member.Name;

            throw new NotSupportedException("Only property accessors are supported for the left-hand side of the expression at this time.");
        }

        static object ParseValueFromExpression(Expression expression)
        {
            var lambdaExpression = Expression.Lambda(expression);
            return lambdaExpression.Compile().DynamicInvoke();
        }
    }
}
