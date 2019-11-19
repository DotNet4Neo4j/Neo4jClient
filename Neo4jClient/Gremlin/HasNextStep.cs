namespace Neo4jClient.Gremlin
{
    public static class HasNextStep
    {
        public static IGremlinNodeQuery<TNode> GremlinHasNext<TNode>(this IGremlinNodeQuery<TNode> query)
        {
            var newQuery = query.AddBlock(".hasNext()");
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery GremlinHasNext(this IGremlinRelationshipQuery query)
        {
            var newQuery = query.AddBlock(".hasNext()");
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> GremlinHasNext<TData>(this IGremlinRelationshipQuery<TData> query)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".hasNext()");
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}