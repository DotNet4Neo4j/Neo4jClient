namespace Neo4jClient.Cypher
{
    public interface IFluentCypherResultItem
    {
        T CollectAs<T>();
    }
}
