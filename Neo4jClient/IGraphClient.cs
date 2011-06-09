namespace Neo4jClient
{
    public interface IGraphClient
    {
        void Connect();
        NodeReference<TNode> Create<TNode>(TNode node, params IRelationshipAllowingParticipantNode<TNode>[] outgoingRelationships) where TNode : class;
        TNode Get<TNode>(NodeReference reference);
        TNode Get<TNode>(NodeReference<TNode> reference);
        RelationshipReference CreateRelationships<TSourceNode>(NodeReference<TSourceNode> node, params IRelationshipAllowingParticipantNode<TSourceNode>[] outgoingRelationship) where TSourceNode : class;
    }
}