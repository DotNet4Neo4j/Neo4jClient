namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryStarted :  ICypherFluentQueryWhere
    {
        ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences);
        ICypherFluentQueryMatched Delete(string identities);
        ICypherFluentQueryMatched Match(params string[] matchText);
        ICypherFluentQueryMatched Relate(string relateText);
        ICypherFluentQueryMatched Create(string createText);
    }
}
