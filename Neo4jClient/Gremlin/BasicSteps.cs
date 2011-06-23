namespace Neo4jClient.Gremlin
{
    public static class BasicSteps
    {
        public static IGremlinQuery OutV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "outV");
            return new GremlinEnumerable<TNode>(queryText);
        }

        public static IGremlinQuery InV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "inV");
            return new GremlinEnumerable<TNode>(queryText);
        }
    }
}