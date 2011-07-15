using System;
using System.Collections.Specialized;
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

        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, NameValueCollection filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var concatenatedFilters = FormatGremlinFilter(filters, comparison);
            var queryText = string.Format("{0}.outV{1}", query.QueryText, concatenatedFilters);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var simpleFilters = new NameValueCollection();
            TranslateFilter(filter, simpleFilters);
            return query.OutV<TNode>(simpleFilters, comparison);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.inV", query.QueryText);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, NameValueCollection filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var concatenatedFilters = FormatGremlinFilter(filters, comparison);
            var queryText = string.Format("{0}.inV{1}", query.QueryText, concatenatedFilters);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var simpleFilters = new NameValueCollection();
            TranslateFilter(filter, simpleFilters);
            return query.InV<TNode>(simpleFilters, comparison);
        }

        internal static string FormatGremlinFilter(NameValueCollection filters, StringComparison comparison)
        {
            string filterFormat, nullFilterFormat, filterSeparator, concatenatedFiltersFormat;
            switch (comparison)
            {
                case StringComparison.Ordinal:
                    filterFormat = "['{0}':'{1}']";
                    nullFilterFormat = "['{0}':null]";
                    filterSeparator = ",";
                    concatenatedFiltersFormat = "[{0}]";
                    break;
                case StringComparison.OrdinalIgnoreCase:
                    filterFormat = "it.'{0}'.equalsIgnoreCase('{1}')";
                    nullFilterFormat = "it.'{0}' == null";
                    filterSeparator = " && ";
                    concatenatedFiltersFormat = "{{ {0} }}";
                    break;
                default:
                    throw new NotSupportedException(string.Format("Comparison mode {0} is not supported.", comparison));
            }

            var formattedFilters = filters
                .AllKeys
                .Select(k => new {
                    Key = k,
                    Value = filters[k],
                    FilterFormat = filters[k] == null ? nullFilterFormat : filterFormat
                })
                .Select(f => string.Format(f.FilterFormat, f.Key, f.Value))
                .ToArray();
            var concatenatedFilters = string.Join(filterSeparator, formattedFilters);
            if (!string.IsNullOrWhiteSpace(concatenatedFilters))
                concatenatedFilters = string.Format(concatenatedFiltersFormat, concatenatedFilters);
            return concatenatedFilters;
        }

        internal static void TranslateFilter<TNode>(Expression<Func<TNode, bool>> filter, NameValueCollection simpleFilters)
        {
            var binaryExpression = filter.Body as BinaryExpression;
            if (binaryExpression == null)
                throw new NotSupportedException("Only binary expressions are supported at this time.");
            if (binaryExpression.NodeType != ExpressionType.Equal)
                throw new NotSupportedException("Only equality expressions are supported at this time.");

            var propertyName = ParseKeyFromExpression(binaryExpression.Left);
            var constantValue = ParseValueFromExpression(binaryExpression.Right);

            simpleFilters.Add(propertyName, constantValue.ToString());
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