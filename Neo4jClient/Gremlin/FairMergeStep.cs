namespace Neo4jClient.Gremlin
{
    public static class FairMergeStep
    {
        public static IGremlinNodeQuery<TNode> FairMerge<TNode>(this IGremlinNodeQuery<TNode> query)
        {
            var newQuery = query.AddBlock(".fairMerge");
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery FairMerge(this IGremlinRelationshipQuery query)
        {
            var newQuery = query.AddBlock(".fairMerge");
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> FairMerge<TData>(this IGremlinRelationshipQuery<TData> query)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".fairMerge");
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}