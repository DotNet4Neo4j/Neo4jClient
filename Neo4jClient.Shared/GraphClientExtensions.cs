using System.Linq;

namespace Neo4jClient
{
    public static class GraphClientExtensions
    {
        public static NodeReference<TNode> Create<TNode>(this IGraphClient graphClient, TNode node, params IRelationshipAllowingParticipantNode<TNode>[] relationships)
            where TNode : class
        {
            return graphClient.Create(node, relationships, Enumerable.Empty<IndexEntry>());
        }
    }
}