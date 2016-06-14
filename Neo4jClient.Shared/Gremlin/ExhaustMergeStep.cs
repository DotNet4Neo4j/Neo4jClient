namespace Neo4jClient.Gremlin
{
    public static class ExhaustMergeStep
    {
        public static IGremlinNodeQuery<TNode> ExhaustMerge<TNode>(this IGremlinNodeQuery<TNode> query)
        {
            var newQuery = query.AddBlock(".exhaustMerge");
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery ExhaustMerge(this IGremlinRelationshipQuery query)
        {
            var newQuery = query.AddBlock(".exhaustMerge");
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> ExhaustMerge<TData>(this IGremlinRelationshipQuery<TData> query)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".exhaustMerge");
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}