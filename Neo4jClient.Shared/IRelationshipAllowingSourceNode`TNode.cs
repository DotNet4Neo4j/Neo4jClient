namespace Neo4jClient
{
    public interface IRelationshipAllowingSourceNode<out TNode>
        : IRelationshipAllowingParticipantNode<TNode>
    {
    }
}