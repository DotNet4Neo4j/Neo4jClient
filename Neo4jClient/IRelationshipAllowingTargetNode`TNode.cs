namespace Neo4jClient
{
    public interface IRelationshipAllowingTargetNode<in TNode>
        : IRelationshipAllowingParticipantNode<TNode>
    {
    }
}