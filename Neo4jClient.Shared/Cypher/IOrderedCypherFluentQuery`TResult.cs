namespace Neo4jClient.Cypher
{
    public interface IOrderedCypherFluentQuery<TResult> : IOrderedCypherFluentQuery, ICypherFluentQuery<TResult>
    {
        new IOrderedCypherFluentQuery<TResult> ThenBy(params string[] properties);
        new IOrderedCypherFluentQuery<TResult> ThenByDescending(params string[] properties);
    }
}