namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQuery
    {
        ICypherQuery Query { get; }
    }
}