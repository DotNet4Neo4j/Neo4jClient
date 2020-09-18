// using System;
// using System.Threading.Tasks;
// using Neo4jClient.Tests.Domain;
// using Neo4jClient.Tests.Relationships;
//
// namespace Neo4jClient.Tests
// {
//     // This class just documents how the API could be consumed. It was an
//     // initial scratching ground before any of the original signatures,
//     // interfaces, or functionality was put in place. Right now it has no
//     // other requirements than just compiling (so as to assert backwards
//     // compatibility with consumers). It is mainly kept for historical
//     // purposes.
//     class ApiUsageIdeas
//     {
//         async Task Foo()
//         {
//             IGraphClient graph = new GraphClient(new Uri(""));
//
//             // Based on http://wiki.neo4j.org/content/Image:Warehouse.png
//
//             // Can create nodes from POCOs
//             var frameStore = await graph.CreateAsync(
//                 new StorageLocation { Name = "Frame Store" });
//             var mainStore = await graph.CreateAsync(
//                 new StorageLocation { Name = "Main Store" });
//
//             // Can create a node with outgoing relationships
//             var frame = await graph.CreateAsync(
//                 new Part { Name = "Frame" },
//                 new StoredIn(frameStore));
//
//             // Can create multiple outgoing relationships and relationships with payloads
//             await graph.CreateAsync(
//                 new Product { Name = "Trike", Weight = 2 },
//                 new StoredIn(mainStore),
//                 new Requires(frame, new Requires.Payload { Count = 1 }));
//
//             // Can create relationships in both directions
//             await graph.CreateAsync(
//                 new Part { Name = "Pedal" },
//                 new StoredIn(frameStore),
//                 new Requires(frame, new Requires.Payload { Count = 2 })
//                     { Direction = RelationshipDirection.Incoming });
//
//             var wheel = await graph.CreateAsync(
//                  new Part { Name = "Wheel" },
//                  new Requires(frame, new Requires.Payload { Count = 2 })
//                     { Direction = RelationshipDirection.Incoming });
//
//             // Can create implicit incoming relationships
//             await graph.CreateAsync(
//                 new StorageLocation { Name = "Wheel Store" },
//                 new StoredIn(wheel));
//
//             // Can create relationships against the root node
//             await graph.CreateAsync(
//                 new StorageLocation {Name = "Auxillary Store"},
//                 new StoredIn(wheel),
//                 new OwnedBy(graph.RootNode));
//         }
//     }
// }