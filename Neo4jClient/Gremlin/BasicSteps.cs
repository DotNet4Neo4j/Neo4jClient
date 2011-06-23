namespace Neo4jClient.Gremlin
{
    public static class BasicSteps
    {
        public static IGremlinQuery OutV(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "outV");
            return new GremlinEnumerable(queryText);
        }

        public static IGremlinQuery OutE(this IGremlinQuery query, string label)
        {
            var queryText = string.Format("{0}.outE[[label:'{1}']]", query.QueryText, label);
            return new GremlinEnumerable(queryText);
        }
    }
}