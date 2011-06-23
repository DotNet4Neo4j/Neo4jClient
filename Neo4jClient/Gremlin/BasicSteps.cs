namespace Neo4jClient.Gremlin
{
    public static class BasicSteps
    {
        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "outV");
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "inV");
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinReferenceQuery OutE(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "outE");
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }

        public static IGremlinReferenceQuery InE(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "inE");
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }
    }
}