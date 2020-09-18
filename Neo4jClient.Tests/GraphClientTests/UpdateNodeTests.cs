// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Threading.Tasks;
// using Xunit;
//
// namespace Neo4jClient.Tests.GraphClientTests
// {
//     
//     public class UpdateNodeTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         [Fact]
//         public async Task ShouldUpdateNode()
//         {
//             var nodeToUpdate = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
//
//             using (var testHarness = new RestTestHarness
//                 {
//                     {
//                         MockRequest.Get("/node/456"),
//                         MockResponse.Json(HttpStatusCode.OK, @"{ 'self': 'http://foo/db/data/node/456',
//                           'data': { 'Foo': 'foo',
//                                     'Bar': 'bar',
//                                     'Baz': 'baz'
//                           },
//                           'create_relationship': 'http://foo/db/data/node/456/relationships',
//                           'all_relationships': 'http://foo/db/data/node/456/relationships/all',
//                           'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
//                           'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
//                           'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
//                           'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
//                           'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
//                           'properties': 'http://foo/db/data/node/456/properties',
//                           'property': 'http://foo/db/data/node/456/property/{key}',
//                           'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
//                         }")
//                     },
//                                         {
//                         MockRequest.PutObjectAsJson("/node/456/properties", nodeToUpdate),
//                         MockResponse.Http((int)HttpStatusCode.NoContent)
//                     }
//                 })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 var pocoReference = new NodeReference<TestNode>(456);
//                 await graphClient.UpdateAsync(
//                     pocoReference, nodeFromDb =>
//                     {
//                         nodeFromDb.Foo = "fooUpdated";
//                         nodeFromDb.Baz = "bazUpdated";
//                         nodeToUpdate = nodeFromDb;
//                     }
//                     );
//
//                 Assert.Equal("fooUpdated", nodeToUpdate.Foo);
//                 Assert.Equal("bazUpdated", nodeToUpdate.Baz);
//                 Assert.Equal("bar", nodeToUpdate.Bar);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReturnNodeAfterUpdating()
//         {
//             var nodeToUpdate = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
//
//             using (var testHarness = new RestTestHarness
//                 {
//                     {
//                         MockRequest.Get("/node/456"),
//                         MockResponse.Json(HttpStatusCode.OK, @"{ 'self': 'http://foo/db/data/node/456',
//                           'data': { 'Foo': 'foo',
//                                     'Bar': 'bar',
//                                     'Baz': 'baz'
//                           },
//                           'create_relationship': 'http://foo/db/data/node/456/relationships',
//                           'all_relationships': 'http://foo/db/data/node/456/relationships/all',
//                           'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
//                           'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
//                           'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
//                           'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
//                           'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
//                           'properties': 'http://foo/db/data/node/456/properties',
//                           'property': 'http://foo/db/data/node/456/property/{key}',
//                           'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
//                         }")
//                     },
//                                         {
//                         MockRequest.PutObjectAsJson("/node/456/properties", nodeToUpdate),
//                         MockResponse.Http((int)HttpStatusCode.NoContent)
//                     }
//                 })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 var pocoReference = new NodeReference<TestNode>(456);
//                 var updatedNode = await graphClient.UpdateAsync(
//                     pocoReference, nodeFromDb =>
//                     {
//                         nodeFromDb.Foo = "fooUpdated";
//                         nodeFromDb.Baz = "bazUpdated";
//                     });
//
//                 Assert.Equal(pocoReference, updatedNode.Reference);
//                 Assert.Equal("fooUpdated", updatedNode.Data.Foo);
//                 Assert.Equal("bazUpdated", updatedNode.Data.Baz);
//                 Assert.Equal("bar", updatedNode.Data.Bar);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldUpdateNodeWithIndexEntries()
//         {
//             var nodeToUpdate = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
//
//             using (var testHarness = new RestTestHarness
//                 {
//                     {
//                         MockRequest.Get("/node/456"),
//                         MockResponse.Json(HttpStatusCode.OK, @"{ 'self': 'http://foo/db/data/node/456',
//                           'data': { 'Foo': 'foo',
//                                     'Bar': 'bar',
//                                     'Baz': 'baz'
//                           },
//                           'create_relationship': 'http://foo/db/data/node/456/relationships',
//                           'all_relationships': 'http://foo/db/data/node/456/relationships/all',
//                           'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
//                           'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
//                           'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
//                           'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
//                           'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
//                           'properties': 'http://foo/db/data/node/456/properties',
//                           'property': 'http://foo/db/data/node/456/property/{key}',
//                           'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
//                         }")
//                     },
//                     {
//                         MockRequest.PutObjectAsJson("/node/456/properties", nodeToUpdate),
//                         MockResponse.Http((int)HttpStatusCode.NoContent)
//                     },
//                     {
//                         MockRequest.Delete("/index/node/foo/456"),
//                         MockResponse.Http((int)HttpStatusCode.NoContent)
//                     },
//                     {
//                         MockRequest.PostObjectAsJson("/index/node/foo", new { key="foo", value="bar", uri="http://foo/db/data/node/456"}),
//                         MockResponse.Json(HttpStatusCode.Created, "Location: http://foo/db/data/index/node/foo/bar/456")
//                     }
//                 })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 // Act
//                 var pocoReference = new NodeReference<TestNode>(456);
//                 await graphClient.UpdateAsync(
//                     pocoReference, nodeFromDb =>
//                     {
//                         nodeFromDb.Foo = "fooUpdated";
//                         nodeFromDb.Baz = "bazUpdated";
//                         nodeToUpdate = nodeFromDb;
//                     }, nodeFromDb => new List<IndexEntry>
//                     {
//                         new IndexEntry
//                         {
//                             Name = "foo", 
//                             KeyValues = new Dictionary<string, object> {{"foo", "bar"}},
//                         }
//                     });
//
//                 Assert.Equal("fooUpdated", nodeToUpdate.Foo);
//                 Assert.Equal("bazUpdated", nodeToUpdate.Baz);
//                 Assert.Equal("bar", nodeToUpdate.Bar);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldRunDelegateForChanges()
//         {
//             var nodeToUpdate = new TestNode { Id = 1, Foo = "foo", Bar = "bar", Baz = "baz" };
//
//             using (var testHarness = new RestTestHarness
//                 {
//                     {
//                         MockRequest.Get("/node/456"),
//                         MockResponse.Json(HttpStatusCode.OK, @"{ 'self': 'http://foo/db/data/node/456',
//                           'data': { 'Foo': 'foo',
//                                     'Bar': 'bar',
//                                     'Baz': 'baz'
//                           },
//                           'create_relationship': 'http://foo/db/data/node/456/relationships',
//                           'all_relationships': 'http://foo/db/data/node/456/relationships/all',
//                           'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
//                           'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
//                           'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
//                           'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
//                           'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
//                           'properties': 'http://foo/db/data/node/456/properties',
//                           'property': 'http://foo/db/data/node/456/property/{key}',
//                           'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
//                         }")
//                     },
//                                         {
//                         MockRequest.PutObjectAsJson("/node/456/properties", nodeToUpdate),
//                         MockResponse.Http((int)HttpStatusCode.NoContent)
//                     }
//                 })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 var hasChanged = false;
//
//                 var pocoReference = new NodeReference<TestNode>(456);
//                 await graphClient.UpdateAsync(
//                     pocoReference, nodeFromDb =>
//                     {
//                         nodeFromDb.Foo = "fooUpdated";
//                         nodeFromDb.Baz = "bazUpdated";
//                         nodeToUpdate = nodeFromDb;
//                     },
//                     null,
//                     diff => { hasChanged = diff.Any(); }
//                 );
//
//                 Assert.True(hasChanged);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReplaceNode()
//         {
//             var newData = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
//
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.PutObjectAsJson("/node/456/properties", newData),
//                     MockResponse.Http((int)HttpStatusCode.NoContent)
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 var pocoReference = new NodeReference<TestNode>(456);
//                 await graphClient.UpdateAsync(pocoReference, newData);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReplaceNodeWithIndexEntries()
//         {
//             var newData = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
//
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.PutObjectAsJson("/node/456/properties", newData),
//                     MockResponse.Http((int)HttpStatusCode.NoContent)
//                 },
//                 {
//                     MockRequest.Delete("/index/node/foo/456"),
//                     MockResponse.Http((int)HttpStatusCode.NoContent)
//                 },
//                 {
//                     MockRequest.PostObjectAsJson("/index/node/foo", new { key="foo", value="bar", uri="http://foo/db/data/node/456"}),
//                     MockResponse.Json(HttpStatusCode.Created, "Location: http://foo/db/data/index/node/foo/bar/456")
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 // Act
//                 var pocoReference = new NodeReference<TestNode>(456);
//                 await graphClient.UpdateAsync(
//                     pocoReference,
//                     newData,
//                     new []
//                     {
//                         new IndexEntry
//                         {
//                             Name = "foo",
//                             KeyValues = new Dictionary<string, object> {{"foo", "bar"}},
//                         }
//                     });
//             }
//         }
//
//         public class TestNode
//         {
//             public int Id { get; set; }
//             public string Foo { get; set; }
//             public string Bar { get; set; }
//             public string Baz { get; set; }
//         }
//     }
// }