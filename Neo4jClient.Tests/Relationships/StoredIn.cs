using Neo4jClient.Test.Domain;

namespace Neo4jClient.Test.Relationships
{
    public class StoredIn :
        Relationship,
        IRelationshipAllowingSourceNode<Part>,
        IRelationshipAllowingSourceNode<Product>,
        IRelationshipAllowingTargetNode<StorageLocation>
    {
        public StoredIn(NodeReference otherNode)
            : base(otherNode)
        {}

        public override string RelationshipTypeKey
        {
            get { return "STORED_IN"; }
        }
    }
}