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
            const string plea = " If you'd like to use something more complex, feel free to contribute a patch to the project.";

            var binaryExpression = filter.Body as BinaryExpression;
            if (binaryExpression == null)
                throw new NotSupportedException("Only binary expressions are supported at this time." + plea);
            if (binaryExpression.NodeType != ExpressionType.Equal)
                throw new NotSupportedException("Only equality expressions are supported at this time." + plea);

            var memberExpression = binaryExpression.Left as MemberExpression;
            if (memberExpression == null)
                throw new NotSupportedException("Only member expressions are supported as the left-hand side of an expression at this time." + plea);
            if (memberExpression.Member.MemberType != MemberTypes.Property)
                throw new NotSupportedException("Only properties are supported as the left-hand side of an expression at this time." + plea);

            var propertyName = memberExpression.Member.Name;

            var constantExpression = binaryExpression.Right as ConstantExpression;
            if (constantExpression == null)
                throw new NotSupportedException("Only constant expressions are supported as the right-hand side of an expression at this time." + plea);

            var constantValue = constantExpression.Value;

            simpleFilters.Add(propertyName, constantValue.ToString());
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

        public static int Count(this IGremlinQuery query)
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