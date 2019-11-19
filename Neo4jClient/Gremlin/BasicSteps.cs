using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Neo4jClient.Gremlin
{
    public static class BasicSteps
    {
        const string bothV = ".bothV";
        const string outV = ".outV";
        const string inV = ".inV";
        const string bothE = ".bothE";
        const string outE = ".outE";
        const string inE = ".inE";
        const string both = ".both({0})";
        const string @out = ".out({0})";
        const string @in = ".in({0})";

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> BothV<TNode>(this IGremlinQuery query)
        {
            var newQuery = query.AddBlock(bothV);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> BothV<TNode>(this IGremlinQuery query, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddFilterBlock(bothV, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> BothV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var filters = FilterFormatters.TranslateFilter(filter);
            return query.BothV<TNode>(filters, comparison);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query)
        {
            var newQuery = query.AddBlock(outV);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddFilterBlock(outV, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var filters = FilterFormatters.TranslateFilter(filter);
            return query.OutV<TNode>(filters, comparison);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query)
        {
            var newQuery = query.AddBlock(inV);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddFilterBlock(inV, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var filters = FilterFormatters.TranslateFilter(filter);
            return query.InV<TNode>(filters, comparison);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery BothE(this IGremlinQuery query)
        {
            var newQuery = query.AddBlock(bothE);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery BothE(this IGremlinQuery query, string label)
        {
            var filter = GetFilter(label);

            var newQuery = query.AddFilterBlock(bothE, new[] { filter }, StringComparison.Ordinal);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> BothE<TData>(this IGremlinQuery query)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(bothE);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> BothE<TData>(this IGremlinQuery query, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where TData : class, new()
        {
            var newQuery = query.AddFilterBlock(bothE, filters, comparison);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> BothE<TData>(this IGremlinQuery query, Expression<Func<TData, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where TData : class, new()
        {
            var filters = FilterFormatters.TranslateFilter(filter);
            return query.BothE<TData>(filters, comparison);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> BothE<TData>(this IGremlinQuery query, string label)
            where TData : class, new()
        {
            return query.BothE<TData>(label, new Filter[0], StringComparison.Ordinal);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> BothE<TData>(this IGremlinQuery query, string label, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where TData : class, new()
        {
            var filter = GetFilter(label);

            filters = filters.Concat(new[] { filter });

            var newQuery = query.AddFilterBlock(bothE, filters, comparison);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> BothE<TData>(this IGremlinQuery query, string label, Expression<Func<TData, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where TData : class, new()
        {
            var filters = FilterFormatters.TranslateFilter(filter);
            return query.BothE<TData>(label, filters, comparison);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery OutE(this IGremlinQuery query)
        {
            var newQuery = query.AddBlock(outE);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery OutE(this IGremlinQuery query, string label)
        {
            var filter = GetFilter(label);

            var newQuery = query.AddFilterBlock(outE, new[] { filter }, StringComparison.Ordinal);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> OutE<TData>(this IGremlinQuery query)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(outE);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> OutE<TData>(this IGremlinQuery query, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where TData : class, new()
        {
            var newQuery = query.AddFilterBlock(outE, filters, comparison);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> OutE<TData>(this IGremlinQuery query, Expression<Func<TData, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where TData : class, new()
        {
            var filters = FilterFormatters.TranslateFilter(filter);
            return query.OutE<TData>(filters, comparison);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> OutE<TData>(this IGremlinQuery query, string label)
            where TData : class, new()
        {
            return query.OutE<TData>(label, new Filter[0], StringComparison.Ordinal);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> OutE<TData>(this IGremlinQuery query, string label, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where TData : class, new()
        {
            var filter = GetFilter(label);

            filters = filters.Concat(new[] { filter });

            var newQuery = query.AddFilterBlock(outE, filters, comparison);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> OutE<TData>(this IGremlinQuery query, string label, Expression<Func<TData, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where TData : class, new()
        {
            var filters = FilterFormatters.TranslateFilter(filter);
            return query.OutE<TData>(label, filters, comparison);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery InE(this IGremlinQuery query)
        {
            var newQuery = query.AddBlock(inE);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery InE(this IGremlinQuery query, string label)
        {
            var filter = GetFilter(label);

            var newQuery = query.AddFilterBlock(inE, new[] { filter }, StringComparison.Ordinal);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> In<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(@in, label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> InE<TData>(this IGremlinQuery query)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(inE);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinRelationshipQuery<TData> InE<TData>(this IGremlinQuery query, string label)
            where TData : class, new()
        {
            var filter = GetFilter(label);

            var newQuery = query.AddFilterBlock(inE, new[] { filter }, StringComparison.Ordinal);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> Both<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(both, label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> Both<TNode>(this IGremlinQuery query, string label, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddBlock(both, label);
            var filterQuery = newQuery.AddFilterBlock(string.Empty, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(filterQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> Both<TNode>(this IGremlinQuery query, string label, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddBlock(both, label);
            var filters = FilterFormatters.TranslateFilter(filter);
            var filterQuery = newQuery.AddFilterBlock(string.Empty, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(filterQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> Out<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(@out, label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> Out<TNode>(this IGremlinQuery query, string label, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddBlock(@out, label);
            var filterQuery = newQuery.AddFilterBlock(string.Empty, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(filterQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> Out<TNode>(this IGremlinQuery query, string label, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddBlock(@out, label);
            var filters = FilterFormatters.TranslateFilter(filter);
            var filterQuery = newQuery.AddFilterBlock(string.Empty, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(filterQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> In<TNode>(this IGremlinQuery query, string label, IEnumerable<Filter> filters, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddBlock(@in, label);
            var filterQuery = newQuery.AddFilterBlock(string.Empty, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(filterQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static IGremlinNodeQuery<TNode> In<TNode>(this IGremlinQuery query, string label, Expression<Func<TNode, bool>> filter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var newQuery = query.AddBlock(@in, label);
            var filters = FilterFormatters.TranslateFilter(filter);
            var filterQuery = newQuery.AddFilterBlock(string.Empty, filters, comparison);
            return new GremlinNodeEnumerable<TNode>(filterQuery);
        }

        [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
        public static int GremlinCount(this IGremlinQuery query)
        {
            if (query.Client == null)
                throw new DetachedNodeException();

            var queryText = string.Format("{0}.count()", query.QueryText);
            var scalarResult = query.Client.ExecuteScalarGremlin(queryText, query.QueryParameters);

            int result;
            if (!int.TryParse(scalarResult, out result))
                throw new Exception(string.Format(
                    "Query returned an unexpected value. Expected an integer. Received: {0}",
                    scalarResult));

            return result;
        }

        static Filter GetFilter(string label)
        {
            // TODO: This filter should always be case sensitive, irrespective of how the rest are compared
            var filter = new Filter
            {
                ExpressionType = ExpressionType.Equal,
                PropertyName = "label",
                Value = label
            };
            return filter;
        }
    }
}