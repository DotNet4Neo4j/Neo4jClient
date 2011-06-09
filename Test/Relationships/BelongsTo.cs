using Neo4jClient.Test.Domain;

namespace Neo4jClient.Test.Relationships
{
    public class BelongsTo : Relationship,

        IRelationshipAllowingSourceNode<Organisation>,
        IRelationshipAllowingSourceNode<SecurityGroup>,
        IRelationshipAllowingSourceNode<User>,

        IRelationshipAllowingTargetNode<Organisation>,
        IRelationshipAllowingTargetNode<SecurityGroup>

    {
        public BelongsTo(NodeReference<Organisation> organisation)
            : base(organisation)
        { }

        public BelongsTo(NodeReference<SecurityGroup> securityGroup)
            : base(securityGroup)
        { }

        public BelongsTo(NodeReference<User> user)
            : base(user)
        { }

        public override string RelationshipTypeKey
        {
            get { return "BELONGS_TO"; }
        }
    }
}