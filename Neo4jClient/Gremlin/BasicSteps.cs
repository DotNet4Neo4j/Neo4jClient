using System;
using System.Collections.Generic;
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

        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, NameValueCollection filters)
        {
            var formattedFilters = filters
                .AllKeys
                .Select(k => new KeyValuePair<string, string>(k, filters[k]))
                .Select(f => string.Format("['{0}':'{1}']", f.Key, f.Value))
                .ToArray();
            var queryText = string.Format("{0}.outV[{1}]", query.QueryText, string.Join(",", formattedFilters));
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter)
        {
            var simpleFilters = new NameValueCollection();
            TranslateFilter(filter, simpleFilters);
            return query.OutV<TNode>(simpleFilters);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.inV", query.QueryText);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, NameValueCollection filters)
        {
            var formattedFilters = filters
                .AllKeys
                .Select(k => new KeyValuePair<string, string>(k, filters[k]))
                .Select(f => string.Format("['{0}':'{1}']", f.Key, f.Value))
                .ToArray();
            var queryText = string.Format("{0}.inV[{1}]", query.QueryText, string.Join(",", formattedFilters));
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter)
        {
            var simpleFilters = new NameValueCollection();
            TranslateFilter(filter, simpleFilters);
            return query.InV<TNode>(simpleFilters);
        }

        static void TranslateFilter<TNode>(Expression<Func<TNode, bool>> filter, NameValueCollection simpleFilters)
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

            throw new NotSupportedException("The left-hand side of the expression contains nodes that aren't yet supported.");
        }

        static string ParseValueFromExpression(Expression expression)
        {
            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
                return constantExpression.Value.ToString();

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null &&
                memberExpression.Expression is ConstantExpression)
            {
                var fieldInfo = (FieldInfo) memberExpression.Member;
                var memberConstantExpression = (ConstantExpression) memberExpression.Expression;
                var value = fieldInfo.GetValue(memberConstantExpression.Value);
                return value.ToString();
            }

            throw new NotSupportedException("The right-hand side of the expression contains nodes that aren't yet supported.");
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
            var scalarResult = query.Client.ExecuteScalarGremlin(queryText, new NameValueCollection());

            int result;
            if (!int.TryParse(scalarResult, out result))
                throw new ApplicationException(string.Format(
                    "Query returned an unexpected value. Expected an integer. Received: {0}",
                    scalarResult));

            return result;
        }
    }
}