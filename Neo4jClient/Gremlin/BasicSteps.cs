using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

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