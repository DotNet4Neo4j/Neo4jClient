namespace Neo4jClient.Gremlin
{
    public static class EmitPropertyStep
    {
        public static IGremlinNodeQuery<TNode> EmitProperty<TNode>(this IGremlinNodeQuery<TNode> query, string propertyName)
        {
            var newQuery = query.AddBlock(string.Format(".{0}", propertyName));
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery EmitProperty(this IGremlinRelationshipQuery query, string propertyName)
        {
            var newQuery = query.AddBlock(string.Format(".{0}", propertyName));
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> EmitProperty<TData>(this IGremlinRelationshipQuery<TData> query, string propertyName)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(string.Format(".{0}", propertyName));
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}