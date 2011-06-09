namespace Neo4jClient
{
    public interface IGraphClient
    {
        void Connect();
        NodeReference Create<TNode>(TNode node, params IRelationship<TNode>[] outgoingRelationships) where TNode : class;
        TNode Get<TNode>(NodeReference reference);
        RelationshipReference CreateRelationship<TRelationship>(NodeReference sourceNode, NodeReference targetNode) where TRelationship : IRelationshipType, new();
        RelationshipReference CreateRelationship<TRelationship, TData>(NodeReference sourceNode, NodeReference targetNode, TData data) where TRelationship : IRelationshipType<TData>, new();
    }
}