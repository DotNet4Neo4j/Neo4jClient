namespace Neo4jClient
{
    public interface IRelationshipAllowingTargetNode<out TNode>
        : IRelationshipAllowingParticipantNode<TNode>
    {
    }
}