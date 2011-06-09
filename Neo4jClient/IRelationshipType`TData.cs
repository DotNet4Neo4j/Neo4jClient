namespace Neo4jClient
{
    public interface IRelationshipType<TData> : IRelationshipType
    {
        TData Data { get; set; }
    }
}