// using System.Linq;
// using System.Threading.Tasks;
//
// namespace Neo4jClient
// {
//     public static class GraphClientExtensions
//     {
//         public static Task<NodeReference<TNode>> CreateAsync<TNode>(this IGraphClient graphClient, TNode node, params IRelationshipAllowingParticipantNode<TNode>[] relationships)
//             where TNode : class
//         {
//             return graphClient.CreateAsync(node, relationships, Enumerable.Empty<IndexEntry>());
//         }
//     }
// }