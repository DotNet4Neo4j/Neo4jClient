namespace Neo4jClient
{
    public interface IGraphClient
    {
        void Connect();
        NodeReference<TNode> Create<TNode>(TNode node, params IAllowsSourceNode<TNode>[] outgoingRelationships) where TNode : class;
        TNode Get<TNode>(NodeReference reference);
        TNode Get<TNode>(NodeReference<TNode> reference);
        RelationshipReference CreateOutgoingRelationships<TSourceNode>(NodeReference<TSourceNode> node, params IAllowsSourceNode<TSourceNode>[] outgoingRelationship) where TSourceNode : class;
    }
}