namespace Neo4jClient.Gremlin
{
    public static class AggregateStep
    {
        public static IGremlinNodeQuery<TNode> AggregateV<TNode>(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(".aggregate({0})", variable);
            newQuery = newQuery.PrepentVariableToBlock(string.Format("{0} = [];", variable));
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery AggregateE(this IGremlinQuery query, string variable)
        {
            var newQuery = query.AddBlock(".aggregate({0})", variable);
            newQuery = newQuery.PrepentVariableToBlock(string.Format("{0} = [];", variable));
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> AggregateE<TData>(this IGremlinQuery query, string variable)
             where TData : class, new()
        {
            var newQuery = query.AddBlock(".aggregate({0})", variable);
            newQuery = newQuery.PrepentVariableToBlock(string.Format("{0} = [];", variable));
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}