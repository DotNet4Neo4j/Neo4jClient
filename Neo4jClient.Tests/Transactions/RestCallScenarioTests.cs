// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Threading.Tasks;
// using Neo4jClient.Tests.Relationships;
// using Neo4jClient.Transactions;
// using Xunit;
//
// namespace Neo4jClient.Tests.Transactions
// {
//     
//     public class RestCallScenarioTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         private class TestNode
//         {
//             public string Foo { get; set; }
//         }
//
//         private async Task ExecuteRestMethodUnderTransaction(
//             Func<IGraphClient, Task> restAction,
//             TransactionScopeOption option = TransactionScopeOption.Join,
//             IEnumerable<KeyValuePair<MockRequest, MockResponse>> requests = null)
//         {
//             requests = requests ?? Enumerable.Empty<KeyValuePair<MockRequest, MockResponse>>();
//             using (var testHarness = new RestTestHarness())
//             {
//                 foreach (var request in requests)
//                 {
//                     testHarness.Add(request.Key, request.Value);
//                 }
//                 var client = await testHarness.CreateAndConnectTransactionalGraphClient();
//                 using (var transaction = client.BeginTransaction(option))
//                 {
//                     await restAction(client);
//                 }
//             }
//         }
//
//         [Fact]
//         public async Task CreateNodeIndexShouldFailUnderTransaction()
//         {
//             await Assert.ThrowsAsync<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(
//                 async client => await client.CreateIndexAsync("node", new IndexConfiguration(), IndexFor.Node)));
//         }
//
//         [Fact]
//         public async Task CreateRelationshipIndexShouldFailUnderTransaction()
//         {
//             await Assert.ThrowsAsync<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(
//                 async client => await client.CreateIndexAsync("rel", new IndexConfiguration(), IndexFor.Relationship)));
//         }
//
//         [Fact]
//         public async Task CreateRelationshipShouldFailUnderTransaction()
//         {
//             var nodeReference = new NodeReference<RootNode>(1);
//             var rel = new OwnedBy(new NodeReference<TestNode>(3));
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.CreateRelationshipAsync(nodeReference, rel)));
//         }
//
//         [Fact]
//         public async Task CreateShouldFailUnderTransaction()
//         {
//             await Assert.ThrowsAsync<InvalidOperationException>(async () =>
//                 await ExecuteRestMethodUnderTransaction(async client => await client.CreateAsync(new object())));
//         }
//
//         [Fact]
//         public async Task DeleteAndRelationshipsShouldFailUnderTransaction()
//         {
//             var pocoReference = new NodeReference<TestNode>(456);
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.DeleteAsync(pocoReference, DeleteMode.NodeAndRelationships)));
//         }
//
//         [Fact]
//         public async Task DeleteIndexEntriesShouldFailUnderTransaction()
//         {
//             var pocoReference = new NodeReference<TestNode>(456);
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.DeleteIndexEntriesAsync("rel", pocoReference)));
//         }
//
//         [Fact]
//         public async Task DeleteNodeIndexShouldFailUnderTransaction()
//         {
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.DeleteIndexAsync("node", IndexFor.Node)));
//         }
//
//         [Fact]
//         public async Task DeleteNodeShouldFailUnderTransaction()
//         {
//             var pocoReference = new NodeReference<TestNode>(456);
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.DeleteAsync(pocoReference, DeleteMode.NodeOnly)));
//         }
//
//         [Fact]
//         public async Task DeleteRelationshipIndexShouldFailUnderTransaction()
//         {
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.DeleteIndexAsync("rel", IndexFor.Relationship)));
//         }
//
//         [Fact]
//         public async Task DeleteRelationshipShouldFailUnderTransaction()
//         {
//             var relReference = new RelationshipReference(1);
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.DeleteRelationshipAsync(relReference)));
//         }
//
//         [Fact]
//         public async Task GetNodeIndexesShouldFailUnderTransaction()
//         {
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.GetIndexesAsync(IndexFor.Node)));
//         }
//
//         [Fact]
//         public async Task GetNodeShouldFailUnderTransaction()
//         {
//             var nodeReference = new NodeReference<TestNode>(1);
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.GetAsync(nodeReference)));
//         }
//
//         [Fact]
//         public async Task GetRelationshipIndexesShouldUnderTransaction()
//         {
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.GetIndexesAsync(IndexFor.Relationship)));
//         }
//
//         [Fact]
//         public async Task GetRelationshipShouldFailUnderTransaction()
//         {
//             var relReference = new RelationshipReference<TestNode>(1);
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.GetAsync(relReference)));
//         }
//
//         [Fact]
//         public async Task GetShouldSucceedInSuppressedMode()
//         {
//             var nodeReference = new NodeReference<TestNode>(1);
//             var requests = new List<KeyValuePair<MockRequest, MockResponse>>
//             {
//                 new KeyValuePair<MockRequest, MockResponse>(
//                     MockRequest.Get("/node/1"),
//                     MockResponse.Json(HttpStatusCode.OK,
//                         @"{ 'self': 'http://foo/db/data/node/456',
//                           'data': { 'Foo': 'foo'
//                           },
//                           'create_relationship': 'http://foo/db/data/node/1/relationships',
//                           'all_relationships': 'http://foo/db/data/node/1/relationships/all',
//                           'all_typed relationships': 'http://foo/db/data/node/1/relationships/all/{-list|&|types}',
//                           'incoming_relationships': 'http://foo/db/data/node/1/relationships/in',
//                           'incoming_typed relationships': 'http://foo/db/data/node/1/relationships/in/{-list|&|types}',
//                           'outgoing_relationships': 'http://foo/db/data/node/1/relationships/out',
//                           'outgoing_typed relationships': 'http://foo/db/data/node/1/relationships/out/{-list|&|types}',
//                           'properties': 'http://foo/db/data/node/1/properties',
//                           'property': 'http://foo/db/data/node/1/property/{key}',
//                           'traverse': 'http://foo/db/data/node/1/traverse/{returnType}'
//                         }")
//                     )
//             };
//             await ExecuteRestMethodUnderTransaction(
//                 async client => await client.GetAsync(nodeReference),
//                 TransactionScopeOption.Suppress,
//                 requests);
//         }
//
//         [Fact]
//         public async Task ReIndexShouldFailUnderTransaction()
//         {
//             var nodeReference = new NodeReference(1);
//             var indexEntries = new[] {new IndexEntry("node")};
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRestMethodUnderTransaction(async client => await client.ReIndexAsync(nodeReference, indexEntries)));
//         }
//
//         [Fact]
//         public async Task UpdateShouldFailUnderTransaction()
//         {
//             await Assert.ThrowsAsync<InvalidOperationException>(async () =>
//                 await ExecuteRestMethodUnderTransaction(async client =>
//                 {
//                     var pocoReference = new NodeReference<TestNode>(456);
//                     var updatedNode = await client.UpdateAsync(
//                         pocoReference, nodeFromDb => { nodeFromDb.Foo = "fooUpdated"; });
//                 }));
//         }
//     }
// }