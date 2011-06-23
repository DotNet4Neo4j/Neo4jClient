namespace Neo4jClient.Gremlin
{
    public static class GremlinQueryExtensions
    {
        public static IGremlinQuery OutV(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "outV");
            return new GremlinEnumerable(queryText);
        }
    }
}