namespace Neo4jClient
{
    public interface IGraphClient
    {
        NodeReference Create<TNode>(TNode node, params OutgoingRelationship<TNode>[] outgoingRelationships) where TNode : class;
    }
}