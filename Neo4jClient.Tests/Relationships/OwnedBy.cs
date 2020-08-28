// using Neo4jClient.Tests.Domain;
//
// namespace Neo4jClient.Tests.Relationships
// {
//     public class OwnedBy :
//         Relationship,
//         IRelationshipAllowingSourceNode<RootNode>,
//         IRelationshipAllowingTargetNode<StorageLocation>
//     {
//         public OwnedBy(NodeReference otherNode)
//             : base(otherNode)
//         {}
//
//         public override string RelationshipTypeKey
//         {
//             get { return "OWNED_BY"; }
//         }
//     }
// }