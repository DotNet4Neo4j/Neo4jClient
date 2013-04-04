namespace Neo4jClient.Cypher
{
    public static class Node
    {
        public static StartBit ByIndexLookup(string indexName, string propertyName, object value)
        {
            return new StartBit(createParameterCallback =>
                string.Format(
                    "node:{0}({1} = {2})",
                    indexName,
                    propertyName,
                    createParameterCallback(value)));
        }
    }
}
