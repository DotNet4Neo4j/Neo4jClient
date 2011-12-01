namespace Neo4jClient.Gremlin
{
    public static class AggregateStep
    {
        public static IGremlinNodeQuery<TNode> AggregateV<TNode>(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".aggregate({0})", label);
            newQuery = newQuery.PrependToBlock("{0} = [];", label);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery AggregateE(this IGremlinQuery query, string label)
        {
            var newQuery = query.AddBlock(".aggregate({0})", label);
            newQuery = newQuery.PrependToBlock("{0} = [];", label);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> AggregateE<TData>(this IGremlinQuery query, string label)
             where TData : class, new()
        {
            var newQuery = query.AddBlock(".aggregate({0})", label);
            newQuery = newQuery.PrependToBlock("{0} = [];", label);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}