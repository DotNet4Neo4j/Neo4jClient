namespace Neo4jClient
{
    public interface IGraphClient
    {
        void Connect();
        NodeReference Create<TNode>(TNode node, params OutgoingRelationship<TNode>[] outgoingRelationships) where TNode : class;
        TNode Get<TNode>(NodeReference reference);
    }
}