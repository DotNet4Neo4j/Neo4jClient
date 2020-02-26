// using System;
// using System.Net;
// using System.Threading.Tasks;
// using Xunit;
//
// namespace Neo4jClient.Tests.GraphClientTests
// {
//     
//     public class GetNodeTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         [Fact]
//         public async Task ShouldThrowInvalidOperationExceptionIfNotConnected()
//         {
//             var client = new GraphClient(new Uri("http://foo"));
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetAsync<object>((NodeReference)123));
//         }
//
//         [Fact]
//         public async Task ShouldReturnNodeData()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Get("/node/456"),
//                     MockResponse.Json(HttpStatusCode.OK,
//                         @"{ 'self': 'http://foo/db/data/node/456',
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
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 var node = await graphClient.GetAsync<TestNode>((NodeReference)456);
//
//                 Assert.Equal(456, node.Reference.Id);
//                 Assert.Equal("foo", node.Data.Foo);
//                 Assert.Equal("bar", node.Data.Bar);
//                 Assert.Equal("baz", node.Data.Baz);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReturnNodeDataForLongId()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Get("/node/21484836470"),
//                     MockResponse.Json(HttpStatusCode.OK,
//                         @"{ 'self': 'http://foo/db/data/node/21484836470',
//                           'data': { 'Foo': 'foo',
//                                     'Bar': 'bar',
//                                     'Baz': 'baz'
//                           },
//                           'create_relationship': 'http://foo/db/data/node/21484836470/relationships',
//                           'all_relationships': 'http://foo/db/data/node/21484836470/relationships/all',
//                           'all_typed relationships': 'http://foo/db/data/node/21484836470/relationships/all/{-list|&|types}',
//                           'incoming_relationships': 'http://foo/db/data/node/21484836470/relationships/in',
//                           'incoming_typed relationships': 'http://foo/db/data/node/21484836470/relationships/in/{-list|&|types}',
//                           'outgoing_relationships': 'http://foo/db/data/node/21484836470/relationships/out',
//                           'outgoing_typed relationships': 'http://foo/db/data/node/21484836470/relationships/out/{-list|&|types}',
//                           'properties': 'http://foo/db/data/node/21484836470/properties',
//                           'property': 'http://foo/db/data/node/21484836470/property/{key}',
//                           'traverse': 'http://foo/db/data/node/21484836470/traverse/{returnType}'
//                         }")
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 var node = await graphClient.GetAsync<TestNode>((NodeReference)21484836470);
//
//                 Assert.Equal(21484836470, node.Reference.Id);
//                 Assert.Equal("foo", node.Data.Foo);
//                 Assert.Equal("bar", node.Data.Bar);
//                 Assert.Equal("baz", node.Data.Baz);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReturnNodeDataAndDeserializeToEnumType()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Get("/node/456"),
//                     MockResponse.Json(HttpStatusCode.OK,
//                         @"{ 'self': 'http://foo/db/data/node/456',
//                           'data': { 'Foo': 'foo',
//                                     'Status': 'Value1'
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
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 var node = await graphClient.GetAsync<TestNodeWithEnum>((NodeReference)456);
//
//                 Assert.Equal(456, node.Reference.Id);
//                 Assert.Equal("foo", node.Data.Foo);
//                 Assert.Equal(TestEnum.Value1, node.Data.Status);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReturnNodeWithReferenceBackToClient()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Get("/node/456"),
//                     MockResponse.Json(HttpStatusCode.OK,
//                         @"{ 'self': 'http://foo/db/data/node/456',
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
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 var node = await graphClient.GetAsync<TestNode>((NodeReference)456);
//
//                 Assert.Equal(graphClient, ((IAttachedReference) node.Reference).Client);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReturnNullWhenNodeDoesntExist()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Get("/node/456"),
//                     MockResponse.Http(404)
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 var node = await graphClient.GetAsync<TestNode>((NodeReference)456);
//
//                 Assert.Null(node);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReturnNodeDataAndDeserialzedJsonDatesForDateTimeOffsetNullableType()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Get("/node/456"),
//                     MockResponse.Json(HttpStatusCode.OK,
//                         @"{ 'self': 'http://foo/db/data/node/456',
//                           'data': { 'DateOffSet': '/Date(1309421746929+0000)/' },
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
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 var node = await graphClient.GetAsync<TestNode>((NodeReference)456);
//
//                 Assert.NotNull(node.Data.DateOffSet);
//                 Assert.Equal("2011-06-30 08:15:46Z", node.Data.DateOffSet.Value.ToString("u"));
//             }
//         }
//
//         public class TestNode
//         {
//             public string Foo { get; set; }
//             public string Bar { get; set; }
//             public string Baz { get; set; }
//             public DateTimeOffset? DateOffSet { get; set; }
//         }
//
//         public class TestNodeWithEnum
//         {
//             public string Foo { get; set; }
//             public TestEnum Status { get; set; }
//         }
//
//         public enum TestEnum
//         {
//             Value1,
//             Value2
//         }
//     }
// }
