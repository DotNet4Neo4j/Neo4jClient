namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryMatched :  ICypherFluentQueryWhere
    {
        ICypherFluentQueryMatched Relate(string relateText);
        ICypherFluentQueryMatched CreateUnique(string createUniqueText);
        ICypherFluentQueryMatched Create(string createText);
        ICypherFluentQueryMatched Create<TNode>(string identity, TNode node) where TNode : class;
        ICypherFluentQueryMatched Delete(string identities);
        void ExecuteWithoutResults();
    }
}
