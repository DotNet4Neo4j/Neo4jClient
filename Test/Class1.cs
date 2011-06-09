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

            var readify = client.Create(
                new Organisation {Name = "Readify"});
            
            // Can create a node from a POCO
            var erik = client.Create(
                new User { FirstName = "Erik", LastName = "Natoli", DateOfBirth = new DateTime(1988, 1, 1) });

            // Can create a node with outgoing relationships
            var sofia = client.Create(
                new User { FirstName = "Sofia", LastName = "Brighton", DateOfBirth = new DateTime(1969, 6, 25) },
                new HasMet(erik, new HasMet.Payload {DateMet = DateTime.UtcNow}),
                new BelongsTo(readify));

            // Can create a node with incoming relationships
            client.Create(
                new SecurityGroup {Name = "Administrators"},
                new BelongsTo(erik));

            // Can create a node with incoming and outgoing relationships
            client.Create(
                new SecurityGroup { Name = "Power Users" },
                new BelongsTo(erik),
                new BelongsTo(readify));

            // Can create outgoing relationships
            client.CreateRelationships(
                erik,
                new BelongsTo(readify));

            // Can create incoming relationships
            client.CreateRelationships(
                readify,
                new BelongsTo(sofia));

            // Can create self-referencing relationships
            client.CreateRelationships(
                readify,
                new BelongsTo(readify));
        }
    }
}
