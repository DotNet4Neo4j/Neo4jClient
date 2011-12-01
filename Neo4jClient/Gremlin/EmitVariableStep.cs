namespace Neo4jClient.Gremlin
{
    public static class EmitVariableStep
    {
        public static IGremlinNodeQuery<TNode> EmitVariableV<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".{0}", label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery EmitVariableE(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".{0}", label);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> EmitVariableE<TData>(this IGremlinQuery query, string label)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".{0}", label);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}