using System;
using Neo4jClient.Test.Domain;

namespace Neo4jClient.Test.Relationships
{
    public class HasMet :
        Relationship<HasMet.Payload>,
        IRelationshipAllowingSourceNode<User>,
        IRelationshipAllowingTargetNode<User>
    {
        public HasMet(NodeReference<User> otherUser, Payload data)
            : base(otherUser, data)
        {}

        public class Payload
        {
            public DateTime DateMet { get; set; }
        }

        public override string RelationshipTypeKey
        {
            get { return "HAS_MET"; }
        }
    }
}