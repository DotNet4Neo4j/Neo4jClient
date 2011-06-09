namespace Neo4jClient
{
    public class Relationship<TNode, TRelationshipType>
        : IRelationship<TNode>
        where TRelationshipType : IRelationshipType, new()
    {
        IRelationshipType IRelationship<TNode>.Type { get { return new TRelationshipType(); } }
        public NodeReference OtherNode { get; set; }
        public RelationshipDirection Direction { get; set; }
        object IRelationship<TNode>.Data { get { return null; } }
    }
}