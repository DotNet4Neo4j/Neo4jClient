namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryMatched :  ICypherFluentQueryWhere
    {
        ICypherFluentQueryMatched Relate(string relateText);
        ICypherFluentQueryMatched Delete(string identities);
    }
}
