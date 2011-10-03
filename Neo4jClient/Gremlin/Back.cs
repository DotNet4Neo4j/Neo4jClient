namespace Neo4jClient.Gremlin
{
    public static class Back
    {
        public static IGremlinNodeQuery<TNode> BackV<TNode>(this IGremlinQuery query, int pipeCount)
        {
            var newQuery = query.AddBlock(".back({0})", pipeCount);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery BackE(this IGremlinQuery query, int pipeCount)
        {
            var newQuery = query.AddBlock(".back({0})", pipeCount);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> BackE<TData>(this IGremlinQuery query, int pipeCount)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".back({0})", pipeCount);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}
