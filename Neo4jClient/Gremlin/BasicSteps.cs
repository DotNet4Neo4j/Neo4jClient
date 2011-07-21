using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.Gremlin
{
    public static class BasicSteps
    {
        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.outV", query.QueryText);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, IDictionary<string, object> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var concatenatedFilters = FormatGremlinFilter(filters, comparison);
            var queryText = string.Format("{0}.outV{1}", query.QueryText, concatenatedFilters);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var simpleFilters = new Dictionary<string, object>();
            TranslateFilter(filter, simpleFilters);
            return query.OutV<TNode>(simpleFilters, comparison);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.inV", query.QueryText);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, IDictionary<string, object> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var concatenatedFilters = FormatGremlinFilter(filters, comparison);
            var queryText = string.Format("{0}.inV{1}", query.QueryText, concatenatedFilters);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var simpleFilters = new Dictionary<string, object>();
            TranslateFilter(filter, simpleFilters);
            return query.InV<TNode>(simpleFilters, comparison);
        }

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

        public static IGremlinReferenceQuery OutE(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.outE", query.QueryText);
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }

        public static IGremlinReferenceQuery OutE(this IGremlinQuery query, string label)
        {
            var queryText = string.Format("{0}.outE[[label:'{1}']]", query.QueryText, label);
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }

        public static IGremlinReferenceQuery InE(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.inE", query.QueryText);
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }

        public static IGremlinReferenceQuery InE(this IGremlinQuery query, string label)
        {
            var queryText = string.Format("{0}.inE[[label:'{1}']]", query.QueryText, label);
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> Out<TNode>(this IGremlinQuery query, string label)
        {
            return query.OutE(label).InV<TNode>();
        }

        public static IGremlinNodeQuery<TNode> Out<TNode>(this IGremlinQuery query, string label, IDictionary<string, object> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return query.OutE(label).InV<TNode>(filters, comparison);
        }

        public static IGremlinNodeQuery<TNode> Out<TNode>(this IGremlinQuery query, string label, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return query.OutE(label).InV(filter, comparison);
        }

        public static IGremlinNodeQuery<TNode> In<TNode>(this IGremlinQuery query, string label)
        {
            return query.InE(label).OutV<TNode>();
        }

        public static IGremlinNodeQuery<TNode> In<TNode>(this IGremlinQuery query, string label, IDictionary<string, object> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return query.InE(label).OutV<TNode>(filters, comparison);
        }

        public static IGremlinNodeQuery<TNode> In<TNode>(this IGremlinQuery query, string label, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return query.InE(label).OutV(filter, comparison);
        }

        public static int NodeCount(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.count()", query.QueryText);
            var scalarResult = query.Client.ExecuteScalarGremlin(queryText);

            int result;
            if (!int.TryParse(scalarResult, out result))
                throw new ApplicationException(string.Format(
                    "Query returned an unexpected value. Expected an integer. Received: {0}",
                    scalarResult));

            return result;
        }
    }
}