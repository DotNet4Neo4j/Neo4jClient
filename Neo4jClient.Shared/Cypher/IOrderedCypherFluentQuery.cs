namespace Neo4jClient.Cypher
{
    public interface IOrderedCypherFluentQuery : ICypherFluentQuery
    {
        IOrderedCypherFluentQuery ThenBy(params string[] properties);
        IOrderedCypherFluentQuery ThenByDescending(params string[] properties);
    }
}