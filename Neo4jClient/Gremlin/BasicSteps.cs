using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
            var concatenatedFilters = FilterFormatters.FormatGremlinFilter(filters, comparison);
            var queryText = string.Format("{0}.outV{1}", query.QueryText, concatenatedFilters);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var simpleFilters = new Dictionary<string, object>();
            FilterFormatters.TranslateFilter(filter, simpleFilters);
            return query.OutV<TNode>(simpleFilters, comparison);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.inV", query.QueryText);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, IDictionary<string, object> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var concatenatedFilters = FilterFormatters.FormatGremlinFilter(filters, comparison);
            var queryText = string.Format("{0}.inV{1}", query.QueryText, concatenatedFilters);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var simpleFilters = new Dictionary<string, object>();
            FilterFormatters.TranslateFilter(filter, simpleFilters);
            return query.InV<TNode>(simpleFilters, comparison);
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
            if (query.Client == null)
                throw new DetachedNodeException();

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