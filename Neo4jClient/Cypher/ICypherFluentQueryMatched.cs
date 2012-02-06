namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryMatched : ICypherFluentQuery
    {
        ICypherFluentQueryReturned Return(params string[] identities);
        ICypherFluentQueryReturned ReturnDistinct(params string[] identities);
    }
}
