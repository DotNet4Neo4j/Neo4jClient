namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryStarted :  ICypherFluentQueryWhere
    {
        ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences);
        ICypherFluentQueryStarted AddStartPointWithNodeIndexLookup(string identity, string indexName, string key, object value);
        ICypherFluentQueryMatched Delete(string identities);
        ICypherFluentQueryMatched Match(params string[] matchText);
        ICypherFluentQueryMatched Relate(string relateText);
        ICypherFluentQueryMatched CreateUnique(string createUniqueText);
        ICypherFluentQueryMatched Create(string createText);
    }
}
