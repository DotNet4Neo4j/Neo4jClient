using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.Gremlin
{
    internal static class FilterFormatters
    {
        internal static FormattedFilter FormatGremlinFilter(IEnumerable<Filter> filters, StringComparison comparison, IGremlinQuery queryThatTheFilterWillEventuallyBeAddedTo)
        {
            filters = filters
                .Select(f =>
                {
                    if (f.Value != null && f.Value.GetType().GetTypeInfo().IsEnum)
                        f.Value = f.Value.ToString();
                    return f;
                })
                .ToArray();

            if (filters.Any(f => !f.ExpressionType.HasValue))
                throw new ArgumentException("ExpressionType must not be null", "filters");

            var typeFilterFormats = new List<TypeFilter>
            {
                new TypeFilter { Type = null, FilterFormat = "it[{0}] == null", ExpressionType = ExpressionType.Equal },
                new TypeFilter { Type = null, FilterFormat = "it[{0}] != null", ExpressionType = ExpressionType.NotEqual },
                new TypeFilter { Type = typeof(int), FilterFormat = "it[{0}] == {1}", ExpressionType = ExpressionType.Equal },
                new TypeFilter { Type = typeof(int), FilterFormat = "it[{0}] != {1}", ExpressionType = ExpressionType.NotEqual },
                new TypeFilter { Type = typeof(int), FilterFormat = "it[{0}] > {1}", ExpressionType = ExpressionType.GreaterThan},
                new TypeFilter { Type = typeof(int), FilterFormat = "it[{0}] < {1}", ExpressionType = ExpressionType.LessThan},
                new TypeFilter { Type = typeof(int), FilterFormat = "it[{0}] >= {1}", ExpressionType = ExpressionType.GreaterThanOrEqual},
                new TypeFilter { Type = typeof(int), FilterFormat = "it[{0}] <= {1}", ExpressionType = ExpressionType.LessThanOrEqual},
                new TypeFilter { Type = typeof(long), FilterFormat = "it[{0}] == {1}", ExpressionType = ExpressionType.Equal },
                new TypeFilter { Type = typeof(long), FilterFormat = "it[{0}] != {1}", ExpressionType = ExpressionType.NotEqual },
                new TypeFilter { Type = typeof(long), FilterFormat = "it[{0}] > {1}", ExpressionType = ExpressionType.GreaterThan},
                new TypeFilter { Type = typeof(long), FilterFormat = "it[{0}] < {1}", ExpressionType = ExpressionType.LessThan},
                new TypeFilter { Type = typeof(long), FilterFormat = "it[{0}] >= {1}", ExpressionType = ExpressionType.GreaterThanOrEqual},
                new TypeFilter { Type = typeof(long), FilterFormat = "it[{0}] <= {1}", ExpressionType = ExpressionType.LessThanOrEqual},
                new TypeFilter { Type = typeof(bool), FilterFormat = "it[{0}] == {1}", ExpressionType = ExpressionType.Equal },
                new TypeFilter { Type = typeof(bool), FilterFormat = "it[{0}] != {1}", ExpressionType = ExpressionType.NotEqual },
                new TypeFilter { Type = typeof(Guid), FilterFormat = "it[{0}] == {1}", ExpressionType = ExpressionType.Equal },
                new TypeFilter { Type = typeof(Guid), FilterFormat = "it[{0}] != {1}", ExpressionType = ExpressionType.NotEqual },
            };

            const string filterSeparator = " && ";
            const string concatenatedFiltersFormat = "{{ {0} }}";

            switch (comparison)
            {
                case StringComparison.Ordinal:
                    typeFilterFormats.Add(new TypeFilter { Type = typeof(string), FilterFormat = "it[{0}].equals({1})", ExpressionType = ExpressionType.Equal });
                    typeFilterFormats.Add(new TypeFilter { Type = typeof(string), FilterFormat = "!it[{0}].equals({1})", ExpressionType = ExpressionType.NotEqual });
                    break;
                case StringComparison.OrdinalIgnoreCase:
                    typeFilterFormats.Add(new TypeFilter { Type = typeof(string), FilterFormat = "it[{0}].equalsIgnoreCase({1})", ExpressionType = ExpressionType.Equal });
                    typeFilterFormats.Add(new TypeFilter { Type = typeof(string), FilterFormat = "!it[{0}].equalsIgnoreCase({1})", ExpressionType = ExpressionType.NotEqual });
                    break;
                default:
                    throw new NotSupportedException(string.Format("Comparison mode {0} is not supported.", comparison));
            }

            var parameters = new Dictionary<string, object>();
            var nextParameterIndex = queryThatTheFilterWillEventuallyBeAddedTo.QueryParameters.Count;
            Func<object, string> createParameter = value =>
            {
                if (value == null) return "null";
                var paramName = string.Format("p{0}", nextParameterIndex);
                parameters.Add(paramName, value);
                nextParameterIndex++;
                return paramName;
            };

            var expandedFilters =
                from f in filters
                let filterValueType = f.Value == null ? null : f.Value.GetType()
                let typeFilter = typeFilterFormats.SingleOrDefault(tf => tf.Type == filterValueType && tf.ExpressionType == f.ExpressionType)
                let isFilterSupported = typeFilter != null
                let filterFormat = isFilterSupported ? typeFilter.FilterFormat : null
                select new
                {
                    f.PropertyName,
                    PropertyNameParam = createParameter(f.PropertyName),
                    ValueParam = createParameter(f.Value),
                    f.ExpressionType,
                    IsFilterSupported = isFilterSupported,
                    ValueType = filterValueType,
                    Format = filterFormat
                };

            expandedFilters = expandedFilters.ToArray();

            var unsupportedFilters = expandedFilters
                .Where(f => !f.IsFilterSupported)
                .Select(f => f.ValueType == null
                    ? string.Format("{0} with null value and expression {1}", f.PropertyName, f.ExpressionType)
                    : string.Format("{0} of type {1}, with expression {2}", f.PropertyName, f.ValueType.FullName, f.ExpressionType))
                .ToArray();
            if (unsupportedFilters.Any())
                throw new NotSupportedException(string.Format(
                    "One or more of the supplied filters is of an unsupported type or expression. Unsupported filters were: {0}",
                    string.Join(", ", unsupportedFilters)));

            var formattedFilters = expandedFilters
                .Select(f => string.Format(f.Format, f.PropertyNameParam, f.ValueParam))
                .ToArray();
            var concatenatedFilters = string.Join(filterSeparator, formattedFilters);
            if (!string.IsNullOrWhiteSpace(concatenatedFilters))
                concatenatedFilters = string.Format(concatenatedFiltersFormat, concatenatedFilters);

            var filter = string.IsNullOrWhiteSpace(concatenatedFilters) ? string.Empty : ".filter";

            return new FormattedFilter
            {
                FilterText = filter + concatenatedFilters,
                FilterParameters = parameters
            };
        }

        internal static IEnumerable<Filter> TranslateFilter<TNode>(Expression<Func<TNode, bool>> filter)
        {
            if (filter.Body.Type == typeof(bool))
            {
                if (filter.Body.NodeType == ExpressionType.MemberAccess)
                {
                    var expression = filter.Body as MemberExpression;

                    if (expression != null &&
                        (expression.Member is PropertyInfo))// && expression.Member.MemberType == MemberTypes.Property))
                    {
                        var newFilter = new Filter
                        {
                            ExpressionType = ExpressionType.Equal,
                            PropertyName = expression.Member.Name,
                            Value = true
                        };

                        return new List<Filter> { newFilter };
                    }
                }

                if (filter.Body.NodeType == ExpressionType.Not)
                {
                    var expression = filter.Body as UnaryExpression;

                    if (expression != null)
                    {
                        var operand = expression.Operand as MemberExpression;
                        if (operand != null)
                        {
                            var newFilter = new Filter
                            {
                                ExpressionType = ExpressionType.Equal,
                                PropertyName = operand.Member.Name,
                                Value = false
                            };

                            return new List<Filter> { newFilter };
                        }
                    }
                }
            }

            var binaryExpression = filter.Body as BinaryExpression;
            if (binaryExpression == null)
                throw new NotSupportedException("Only binary expressions are supported at this time.");
            return TranslateFilterInternal(binaryExpression);
        }

        static IEnumerable<Filter> TranslateFilterInternal(BinaryExpression binaryExpression)
        {
            switch (binaryExpression.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    var leftBinaryExpression = binaryExpression.Left as BinaryExpression;
                    if (leftBinaryExpression == null)
                        throw new NotSupportedException(string.Format(
                            "This expression is not a binary expression: {0}", binaryExpression.Left));
                    var firstFilter = TranslateFilterInternal(leftBinaryExpression);

                    var rightBinaryExpression = binaryExpression.Right as BinaryExpression;
                    if (rightBinaryExpression == null)
                        throw new NotSupportedException(string.Format(
                            "This expression is not a binary expression: {0}", binaryExpression.Right));
                    var secondFilter = TranslateFilterInternal(rightBinaryExpression);

                    return firstFilter.Concat(secondFilter).ToArray();

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    throw new NotSupportedException(string.Format(
                        "Oprerator {0} is not yet supported. There's no reason why it can't be; we just haven't done it yet. Feel free to send a pull request if you need this feature. It was used in expression: {1}",
                        binaryExpression.NodeType,
                        binaryExpression));
            }

            var key = ParseKeyFromExpression(binaryExpression.Left);
            var constantValue = ParseValueFromExpression(binaryExpression.Right);

            var underlyingPropertyType = key.PropertyType;
            underlyingPropertyType = Nullable.GetUnderlyingType(key.PropertyType) ?? underlyingPropertyType;

            var convertedValue = underlyingPropertyType.GetTypeInfo().IsEnum
                ? Enum.Parse(underlyingPropertyType, constantValue.ToString())
                : constantValue;

            return new[]
            {
                new Filter
                {
                    PropertyName = key.Name,
                    Value = convertedValue,
                    ExpressionType = binaryExpression.NodeType
                }
            };
        }

        internal static ExpressionKey ParseKeyFromExpression(Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null &&
                unaryExpression.NodeType == ExpressionType.Convert)
                expression = unaryExpression.Operand;

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null &&
                memberExpression.Member is PropertyInfo
                //&& memberExpression.Member.MemberType == MemberTypes.Property)
                )
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

        internal class ExpressionKey
        {
            public string Name { get; set; }
            public Type PropertyType { get; set; }
        }
    }
}