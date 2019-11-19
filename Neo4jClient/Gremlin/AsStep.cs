namespace Neo4jClient.Gremlin
{
    public static class AsStep
    {
        public static IGremlinNodeQuery<TNode> As<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".as({0})", label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinNodeQuery<TNode> As<TNode>(this IGremlinNodeQuery<TNode> query, string label)
        {
            var newQuery = query.AddBlock(".as({0})", label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery As(this IGremlinRelationshipQuery query, string label)
        {
            var newQuery = query.AddBlock(".as({0})", label);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> As<TData>(this IGremlinRelationshipQuery<TData> query, string label)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".as({0})", label);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}
