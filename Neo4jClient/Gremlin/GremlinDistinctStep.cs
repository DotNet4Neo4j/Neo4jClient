namespace Neo4jClient.Gremlin
{
    public static class GremlinDistinctStep
    {
        public static IGremlinNodeQuery<TNode> GremlinDistinct<TNode>(this IGremlinNodeQuery<TNode> query)
        {
            var newQuery = query.AddBlock(".dedup()");
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery GremlinDistinct(this IGremlinRelationshipQuery query)
        {
            var newQuery = query.AddBlock(".dedup()");
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> GremlinDistinct<TData>(this IGremlinRelationshipQuery<TData> query)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".dedup()");
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}