namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryStarted :  ICypherFluentQueryWhere
    {
        ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences);
        ICypherFluentQueryMatched Match(string matchText);
    }
}
