namespace Neo4jClient.Gremlin
{
    public static class PrintLineStatement
    {
        public static IGremlinQuery PrintLine(this IGremlinQuery query, string value)
        {
            var newQuery = query.AddBlock(string.Format("println {0}", value));
            return  newQuery;
        }
    }
}