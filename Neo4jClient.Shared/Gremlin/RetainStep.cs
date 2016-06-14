namespace Neo4jClient.Gremlin
{
    public static class RetainStep
    {
        public static IGremlinNodeQuery<TNode> RetainV<TNode>(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".retain({0})", variable));
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery RetainE(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".retain({0})", variable));
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> RetainE<TData>(this IGremlinQuery query, string variable)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(string.Format(".retain({0})", variable));
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}