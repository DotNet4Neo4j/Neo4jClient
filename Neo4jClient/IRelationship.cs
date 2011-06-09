namespace Neo4jClient
{
    public interface IRelationship<T>
    {
        IRelationshipType Type { get; }
        NodeReference OtherNode { get; }
        RelationshipDirection Direction { get; }
        object Data { get; }
    }
}