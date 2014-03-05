namespace Neo4jClient.Execution
{
    /// <summary>
    /// A factory class that returns a policy factory given the type and a <c>IGraphClient</c> connection.
    /// </summary>
    internal interface IExecutionPolicyFactory
    {
        IExecutionPolicy GetPolicy(PolicyType type);
    }

    /// <summary>
    /// Possible enumerations of queries that a policy may represent
    /// </summary>
    public enum PolicyType
    {
        Cypher,
        Gremlin,
        Rest,
        Batch,
        Transaction,
        NodeIndex,
        RelationshipIndex
    }
}