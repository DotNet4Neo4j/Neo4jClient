using System;
using Neo4jClient.Test.Domain;
using Neo4jClient.Test.Relationships;

namespace Neo4jClient.Test
{
    class Class1
    {
        void Foo()
        {
            IGraphClient graph = new GraphClient(new Uri(""));

            // Based on http://wiki.neo4j.org/content/Image:Warehouse.png

            // Can create nodes from POCOs
            var frameStore = graph.Create(
                new StorageLocation {Name = "Frame Store"});
            var mainStore = graph.Create(
                new StorageLocation { Name = "Main Store" });

            // Can create a node with outgoing relationships
            var frame = graph.Create(
                new Part { Name = "Frame" },
                new StoredIn(frameStore));

            // Can create multiple outgoing relationships and relationships with payloads
            graph.Create(
                new Product { Name = "Trike", Weight = 2 },
                new StoredIn(mainStore),
                new Requires(frame, new Requires.Payload { Count = 1 }));

            // Can create relationships in both directions
            graph.Create(
                new Part { Name = "Frame" },
                new StoredIn(frameStore),
                new Requires(frame, new Requires.Payload{ Count = 2 })
                    { Direction = RelationshipDirection.Incoming });

            var wheel = graph.Create(
                 new Part { Name = "Wheel" },
                 new Requires(frame, new Requires.Payload { Count = 2 })
                    { Direction = RelationshipDirection.Incoming });

            // Can create implicit incoming relationships
            graph.Create(
                new StorageLocation { Name = "Wheel Store" },
                new StoredIn(wheel));
        }
    }
}