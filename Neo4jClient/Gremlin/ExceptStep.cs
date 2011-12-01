namespace Neo4jClient.Gremlin
{
    public static class ExceptStep
    {
        public static IGremlinNodeQuery<TNode> ExceptV<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".except({0})", label);
            newQuery = newQuery.PrependToBlock("{0} = [];", label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery ExceptE(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".except({0})", label);
            newQuery = newQuery.PrependToBlock("{0} = [];", label);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> ExceptE<TData>(this IGremlinQuery query, string label)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".except({0})", label);
            newQuery = newQuery.PrependToBlock("{0} = [];", label);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}