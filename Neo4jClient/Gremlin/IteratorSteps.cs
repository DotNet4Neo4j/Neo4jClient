namespace Neo4jClient.Gremlin
{
    public static class Iterator
    {
        public static IGremlinNodeQuery<TNode> GremlinSkipV<TNode>(this IGremlinQuery query, int count)
        {
            var newQuery = query.AddBlock(".drop({0})._()", count);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery GremlinSkipE(this IGremlinQuery query, int count)
        {
            var newQuery = query.AddBlock(".drop({0})._()", count);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> GremlinSkipE<TData>(this IGremlinQuery query, int count)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".drop({0})._()", count);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }

        public static IGremlinNodeQuery<TNode> GremlinTakeV<TNode>(this IGremlinQuery query, int count)
        {
            var newQuery = query.AddBlock(".take({0})._()", count);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery GremlinTakeE(this IGremlinQuery query, int count)
        {
            var newQuery = query.AddBlock(".take({0})._()", count);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> GremlinTakeE<TData>(this IGremlinQuery query, int count)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".take({0})._()", count);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}
