namespace Neo4jClient
{
    public class Relationship<TNode, TRelationshipType, TRelationshipData>
        : IRelationship<TNode>
        where TRelationshipType : IRelationshipType<TRelationshipData>, new()
    {
        IRelationshipType IRelationship<TNode>.Type { get { return new TRelationshipType(); } }
        public NodeReference OtherNode { get; set; }
        public RelationshipDirection Direction { get; set; }
        public TRelationshipData Data { get; set; }
        object IRelationship<TNode>.Data { get { return Data; } }
    }
}