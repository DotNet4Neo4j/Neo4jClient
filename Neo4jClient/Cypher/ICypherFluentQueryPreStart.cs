namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryPreStart : ICypherFluentQuery
    {
        ICypherFluentQueryStarted Start(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQueryStarted Start(string identity, params RelationshipReference[] relationshipReferences);
        ICypherFluentQueryStarted StartWithNodeIndexLookup(string identity, string indexName, string key, object value);
        ICypherFluentQueryStarted StartWithNodeIndexLookup(string identity, string indexName, string parameterText);
    }
}
