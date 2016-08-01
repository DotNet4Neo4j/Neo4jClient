namespace Neo4jClient.Cypher
{
    public enum CypherResultMode
    {
        /// <summary>
        /// In this mode, we expect the Cypher table to contain a single column. When deserializing it,
        /// instead of giving the developer a list of one-column rows, we'll just give them a list of objects.
        /// Effectively, we unwrap the column into a straight up array of entries. This is done to make the
        /// syntax a little nicer when a developer wants to return a single identity and not a full table.
        /// </summary>
        Set,

        /// <summary>
        /// This is the default mode, and tells the serializer to treat each row as one object to deserialize.
        /// </summary>
        Projection
    }
}
