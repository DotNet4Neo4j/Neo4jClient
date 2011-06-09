using System;
using Neo4jClient.Test.Domain;
using Neo4jClient.Test.Relationships;

namespace Neo4jClient.Test
{
    class Class1
    {
        void Foo()
        {
            IGraphClient client = new GraphClient(new Uri(""));

            var barnardos = client.Create(
                new Agency {Name = "Barnardos"});
            
            var bob = client.Create(
                new User {Name = "Bob"});

            client.Create(
                new User {Name = "John"},
                new HasMet(bob, new HasMet.Payload {DateMet = DateTime.UtcNow}),
                new BelongsTo(barnardos));

            client.CreateOutgoingRelationships(
                bob,
                new BelongsTo(barnardos));

            client.CreateOutgoingRelationships(
                barnardos,
                new BelongsTo(barnardos));
        }
    }

    namespace Domain
    {
        public class User
        {
            public string Name { get; set; }
        }

        public class Agency
        {
            public string Name { get; set; }
        }
    }

    namespace Relationships
    {
        public class HasMet :
            Relationship<HasMet.Payload>,
            IAllowsSourceNode<User>,
            IAllowsTargetNode<User>
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

        public class BelongsTo :
            Relationship,
            IAllowsSourceNode<User>,
            IAllowsSourceNode<Agency>,
            IAllowsTargetNode<Agency>
        {
            public BelongsTo(NodeReference<Agency> agency)
                : base(agency)
            { }

            public override string RelationshipTypeKey
            {
                get { return "BELONGS_TO"; }
            }
        }
    }
}
