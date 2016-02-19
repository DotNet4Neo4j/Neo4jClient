namespace Neo4jClient.Gremlin
{
    public static class LoopStep
    {
        public static IGremlinNodeQuery<TNode> LoopV<TNode>(this IGremlinQuery query, string label, uint loopCount)
        {
            var newQuery = query.AddBlock(".loop({0}){{ it.loops < {1} }}", label, loopCount);
            return new GremlinNodeEnumerable<TNode>(newQuery);
        }

        public static IGremlinRelationshipQuery LoopE(this IGremlinQuery query, string label, uint loopCount)
        {
            var newQuery = query.AddBlock(".loop({0}){{ it.loops < {1} }}", label, loopCount);
            return new GremlinRelationshipEnumerable(newQuery);
        }

        public static IGremlinRelationshipQuery<TData> LoopE<TData>(this IGremlinQuery query, string label, uint loopCount)
            where TData : class, new()
        {
            var newQuery = query.AddBlock(".loop({0}){{ it.loops < {1} }}", label, loopCount);
            return new GremlinRelationshipEnumerable<TData>(newQuery);
        }
    }
}
