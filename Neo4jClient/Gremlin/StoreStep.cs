namespace Neo4jClient.Gremlin
{
    public static class StoreStep
    {
        //ToDo Update to use store() in Gremlin 1.4
        public static IGremlinNodeQuery<TNode> StoreV<TNode>(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".sideEffect{0}{1}.add(it){2}", "{",variable, "}"));
            newQuery = newQuery.PrepentVariableToBlock(string.Format("{0} = [];", variable));
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery StoreE(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(string.Format(".sideEffect{0}{1}.add(it){2}", "{", variable, "}"));
            newQuery = newQuery.PrepentVariableToBlock(string.Format("{0} = [];", variable));
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> StoreE<TData>(this IGremlinQuery query, string variable)
             where TData : class, new()
        {
            var newQuery = query.AddBlock(string.Format(".sideEffect{0}{1}.add(it){2}", "{", variable, "}"));
            newQuery = newQuery.PrepentVariableToBlock(string.Format("{0} = [];", variable));
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}