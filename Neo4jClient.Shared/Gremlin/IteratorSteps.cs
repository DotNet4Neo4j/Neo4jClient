namespace Neo4jClient.Gremlin
{
    public static class Iterator
    {
        public static IGremlinNodeQuery<TNode> GremlinSkip<TNode>(this IGremlinNodeQuery<TNode> query, int count)
        {
            var newQuery = query.AddBlock(".drop({0})._()", count);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery GremlinSkip(this IGremlinRelationshipQuery query, int count)
        {
            var newQuery = query.AddBlock(".drop({0})._()", count);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> GremlinSkip<TData>(this IGremlinRelationshipQuery<TData> query, int count)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".drop({0})._()", count);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        public static IGremlinNodeQuery<TNode> GremlinTake<TNode>(this IGremlinNodeQuery<TNode> query, int count)
        {
            var newQuery = query.AddBlock(".take({0})._()", count);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery GremlinTake(this IGremlinRelationshipQuery query, int count)
        {
            var newQuery = query.AddBlock(".take({0})._()", count);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> GremlinTake<TData>(this IGremlinRelationshipQuery<TData> query, int count)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".take({0})._()", count);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}
