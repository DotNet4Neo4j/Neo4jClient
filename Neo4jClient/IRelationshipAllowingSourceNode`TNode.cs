namespace Neo4jClient
{
    public interface IRelationshipAllowingSourceNode<in TNode>
        : IRelationshipAllowingParticipantNode<TNode>
    {
    }
}