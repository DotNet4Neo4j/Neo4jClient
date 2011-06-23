namespace Neo4jClient.Gremlin
{
    public static class BasicSteps
    {
        public static IGremlinNodeQuery<TNode> OutV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.outV", query.QueryText);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinNodeQuery<TNode> InV<TNode>(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.inV", query.QueryText);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinReferenceQuery OutE(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.outE", query.QueryText);
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }

        public static IGremlinReferenceQuery OutE(this IGremlinQuery query, string label)
        {
            var queryText = string.Format("{0}.outE[[label:'{1}']]", query.QueryText, label);
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }

        public static IGremlinReferenceQuery InE(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.inE", query.QueryText);
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }

        public static IGremlinReferenceQuery InE(this IGremlinQuery query, string label)
        {
            var queryText = string.Format("{0}.inE[[label:'{1}']]", query.QueryText, label);
            return new GremlinReferenceEnumerable(query.Client, queryText);
        }
    }
}