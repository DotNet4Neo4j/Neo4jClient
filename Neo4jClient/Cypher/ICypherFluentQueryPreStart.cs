namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryPreStart : ICypherFluentQuery
    {
        ICypherFluentQueryStarted Start(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQueryStarted Start(string identity, params RelationshipReference[] relationshipReferences);
    }
}
