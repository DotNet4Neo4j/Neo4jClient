namespace Neo4jClient.Cypher
{
    public static class Node
    {
        /// <summary>
        /// Used for Cypher <code>START</code> clauses, like <code>Start(new { foo = Node.ByIndexLookup(…) })</code>
        /// </summary>
        public static StartBit ByIndexLookup(string indexName, string propertyName, object value)
        {
            return new StartBit(createParameterCallback =>
                string.Format(
                    "node:`{0}`({1} = {2})",
                    indexName,
                    propertyName,
                    createParameterCallback(value)));
        }

        /// <summary>
        /// Used for Cypher <code>START</code> clauses, like <code>Start(new { foo = Node.ByIndexQuery(…) })</code>
        /// </summary>
        public static StartBit ByIndexQuery(string indexName, string query)
        {
            return new StartBit(createParameterCallback =>
                string.Format(
                    "node:`{0}`({1})",
                    indexName,
                    createParameterCallback(query)));
        }
    }
}
