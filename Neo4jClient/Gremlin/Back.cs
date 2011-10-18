using System;

namespace Neo4jClient.Gremlin
{
    public static class Back
    {
        [Obsolete("Use named backtracks instead. For example, graphClient.RootNode.OutE().As(\"foo\").InV<object>().Back(\"foo\").")]
        public static IGremlinNodeQuery<TNode> BackV<TNode>(this IGremlinQuery query, int pipeCount)
        {
            var newQuery = query.AddBlock(".back({0})", pipeCount);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinNodeQuery<TNode> BackV<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".back({0})", label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        [Obsolete("Use named backtracks instead. For example, graphClient.RootNode.OutE().As(\"foo\").InV<object>().Back(\"foo\").")]
        public static IGremlinRelationshipQuery BackE(this IGremlinQuery query, int pipeCount)
        {
            var newQuery = query.AddBlock(".back({0})", pipeCount);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery BackE(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".back({0})", label);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        [Obsolete("Use named backtracks instead. For example, graphClient.RootNode.OutE().As(\"foo\").InV<object>().Back(\"foo\").")]
        public static IGremlinRelationshipQuery<TData> BackE<TData>(this IGremlinQuery query, int pipeCount)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".back({0})", pipeCount);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> BackE<TData>(this IGremlinQuery query, string label)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".back({0})", label);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}
