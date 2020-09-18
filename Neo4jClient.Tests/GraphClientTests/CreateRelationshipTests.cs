// using System;
// using System.Net;
// using System.Threading.Tasks;
// using Xunit;
//
// namespace Neo4jClient.Tests.GraphClientTests
// {
//     
//     public class CreateRelationshipTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         [Fact]
//         public async Task ShouldReturnRelationshipReference()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.PostJson("/node/81/relationships",
//                         @"{
//                             'to': 'http://foo/db/data/node/81',
//                             'type': 'TEST_RELATIONSHIP'
//                         }"),
//                     MockResponse.Json(HttpStatusCode.Created,
//                         @"{
//                             'extensions' : {
//                             },
//                             'start' : 'http://foo/db/data/node/81',
//                             'property' : 'http://foo/db/data/relationship/38/properties/{key}',
//                             'self' : 'http://foo/db/data/relationship/38',
//                             'properties' : 'http://foo/db/data/relationship/38/properties',
//                             'type' : 'TEST_RELATIONSHIP',
//                             'end' : 'http://foo/db/data/node/80',
//                             'data' : {
//                             }
//                         }")
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 var testRelationship = new TestRelationship(81);
//                 var relationshipReference = await graphClient.CreateRelationshipAsync(new NodeReference<TestNode>(81), testRelationship);
//                 
//                 Assert.IsAssignableFrom<RelationshipReference>(relationshipReference);
//                 Assert.IsNotType<RelationshipReference<object>>(relationshipReference);
//                 Assert.Equal(38, relationshipReference.Id);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReturnAttachedRelationshipReference()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.PostJson("/node/81/relationships",
//                         @"{
//                             'to': 'http://foo/db/data/node/81',
//                             'type': 'TEST_RELATIONSHIP'
//                         }"),
//                     MockResponse.Json(HttpStatusCode.Created,
//                         @"{
//                             'extensions' : {
//                             },
//                             'start' : 'http://foo/db/data/node/81',
//                             'property' : 'http://foo/db/data/relationship/38/properties/{key}',
//                             'self' : 'http://foo/db/data/relationship/38',
//                             'properties' : 'http://foo/db/data/relationship/38/properties',
//                             'type' : 'TEST_RELATIONSHIP',
//                             'end' : 'http://foo/db/data/node/80',
//                             'data' : {
//                             }
//                         }")
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 var testRelationship = new TestRelationship(81);
//                 var relationshipReference = await graphClient.CreateRelationshipAsync(new NodeReference<TestNode>(81), testRelationship);
//
//                 Assert.Equal(graphClient, ((IAttachedReference)relationshipReference).Client);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldThrowArgumentNullExceptionForNullNodeReference()
//         {
//             var client = new GraphClient(new Uri("http://foo"));
//             await Assert.ThrowsAsync<ArgumentNullException>(async() => await client.CreateRelationshipAsync((NodeReference<TestNode>)null, new TestRelationship(10)));
//         }
//
//         [Fact]
//         public async Task ShouldThrowInvalidOperationExceptionIfNotConnected()
//         {
//             var client = new GraphClient(new Uri("http://foo"));
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.CreateRelationshipAsync(new NodeReference<TestNode>(5), new TestRelationship(10)));
//         }
//
//         [Fact]
//         public async Task ShouldThrowNotSupportedExceptionForIncomingRelationship()
//         {
//             using (var testHarness = new RestTestHarness())
//             {
//                 var client = await testHarness.CreateAndConnectGraphClient();
//                 await Assert.ThrowsAsync<NotSupportedException>(async () => await client.CreateRelationshipAsync(new NodeReference<TestNode>(5), new TestRelationship(10) { Direction = RelationshipDirection.Incoming }));
//             }
//         }
//
//         public class TestNode
//         {
//         }
//
//         public class TestNode2
//         {
//         }
//
//         public class TestRelationship : Relationship,
//             IRelationshipAllowingSourceNode<TestNode>,
//             IRelationshipAllowingTargetNode<TestNode2>
//         {
//             public TestRelationship(NodeReference targetNode)
//                 : base(targetNode)
//             {
//             }
//
//             public override string RelationshipTypeKey
//             {
//                 get { return "TEST_RELATIONSHIP"; }
//             }
//         }
//     }
// }
