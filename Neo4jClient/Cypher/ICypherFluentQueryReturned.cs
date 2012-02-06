namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturned : ICypherFluentQuery
    {
        ICypherFluentQueryReturned Limit(int? limit);
    }
}
