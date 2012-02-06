namespace Neo4jClient.Cypher
{
    public interface ICypherResultItem
    {
        T As<T>();
    }
}
