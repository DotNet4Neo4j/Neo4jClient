namespace Neo4jClient.Gremlin
{
    public static class StoreStep
    {
        public static IGremlinNodeQuery<TNode> StoreV<TNode>(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".store({0})", variable));
            newQuery.QueryDeclarations.Add(string.Format("{0} = [];", variable));
            newQuery = newQuery.PrependVariablesToBlock(newQuery);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery StoreE(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".store({0})", variable));
            newQuery.QueryDeclarations.Add(string.Format("{0} = [];", variable));
            newQuery = newQuery.PrependVariablesToBlock(newQuery);

            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> StoreE<TData>(this IGremlinQuery query, string variable)
             where TData : class, new()
        {
            var newQuery = query.AddBlock(string.Format(".store({0})", variable));
            newQuery.QueryDeclarations.Add(string.Format("{0} = [];", variable));
            newQuery = newQuery.PrependVariablesToBlock(newQuery);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}