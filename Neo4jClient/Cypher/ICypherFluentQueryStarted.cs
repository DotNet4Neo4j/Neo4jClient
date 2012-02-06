namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryStarted : ICypherFluentQuery
    {
        ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences);
        ICypherFluentQuery Return(params string[] identities);
    }
}
