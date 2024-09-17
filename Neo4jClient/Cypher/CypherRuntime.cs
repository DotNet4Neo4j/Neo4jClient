namespace Neo4jClient.Cypher
{
    public enum CypherRuntime
    {
        /// <summary>The default for the Community Edition of Neo4j</summary>
        Slotted,
        /// <summary>The default for the Enterprise Edition of Neo4j</summary>
        Pipelined,
        /// <summary>Allows for multi-threaded execution of queries.</summary>
        /// <remarks>This does not always result in increased performance.</remarks>
        Parallel,
    }
}