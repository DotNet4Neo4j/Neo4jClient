namespace Neo4jClient.Gremlin
{
    public static class ExceptStep
    {
        public static IGremlinNodeQuery<TNode> ExceptV<TNode>(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".except({0})", variable));
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery ExceptE(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".except({0})", variable));
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> ExceptE<TData>(this IGremlinQuery query, string variable)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(string.Format(".except({0})", variable));
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}