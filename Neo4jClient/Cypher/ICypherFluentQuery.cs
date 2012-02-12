namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQuery
    {
        CypherQuery Query { get; }
    }
}