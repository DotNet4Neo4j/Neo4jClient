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
    }
}
