using Neo4jClient.Tests.Domain;

namespace Neo4jClient.Tests.Relationships
{
    public class Requires :
        Relationship<Requires.Payload>,
        IRelationshipAllowingSourceNode<Product>,
        IRelationshipAllowingSourceNode<Part>,
        IRelationshipAllowingTargetNode<Part>
    {
        public Requires(NodeReference otherUser, Payload data)
            : base(otherUser, data)
        {}

        public class Payload
        {
            public int Count { get; set; }
        }

        public override string RelationshipTypeKey
        {
            get { return "REQUIRES"; }
        }
    }
}