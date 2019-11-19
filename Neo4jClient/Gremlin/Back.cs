namespace Neo4jClient.Gremlin
{
    public static class Back
    {
        public static IGremlinNodeQuery<TNode> BackV<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".back({0})", label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery BackE(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".back({0})", label);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> BackE<TData>(this IGremlinQuery query, string label)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".back({0})", label);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}
