namespace Neo4jClient.Gremlin
{
    public static class AggregateStep
    {
        public static IGremlinNodeQuery<TNode> AggregateV<TNode>(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".aggregate({0})", variable));
            newQuery.QueryDeclarations.Add(string.Format("{0} = [];", variable));
            newQuery = newQuery.PrependVariablesToBlock(newQuery);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery AggregateE(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".aggregate({0})", variable));
            newQuery.QueryDeclarations.Add(string.Format("{0} = [];", variable));
            newQuery = newQuery.PrependVariablesToBlock(newQuery);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> AggregateE<TData>(this IGremlinQuery query, string variable)
             where TData : class, new()
        {
            var newQuery = query.AddBlock(string.Format(".aggregate({0})", variable));
            newQuery.QueryDeclarations.Add(string.Format("{0} = [];", variable));
            newQuery = newQuery.PrependVariablesToBlock(newQuery);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}