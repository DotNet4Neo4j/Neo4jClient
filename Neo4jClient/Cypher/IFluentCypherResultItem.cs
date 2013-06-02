namespace Neo4jClient.Cypher
{
    public interface IFluentCypherResultItem
    {
        Node<T> CollectAs<T>();
    }
}
