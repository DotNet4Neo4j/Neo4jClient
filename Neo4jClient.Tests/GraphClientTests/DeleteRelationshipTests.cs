// using System;
// using System.Threading.Tasks;
// using FluentAssertions;
// using Xunit;
//
// namespace Neo4jClient.Tests.GraphClientTests
// {
//     
//     public class DeleteRelationshipTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         [Fact]
//         public async Task ShouldThrowInvalidOperationExceptionIfNotConnected()
//         {
//             var client = new GraphClient(new Uri("http://foo"));
//             await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.DeleteRelationshipAsync(123));
//         }
//
//         [Fact]
//         public async Task ShouldDeleteRelationship()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Delete("/relationship/456"),
//                     MockResponse.Http(204)
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 await graphClient.DeleteRelationshipAsync(456);
//             }
//         }
//
//         [Fact]
//         public async Task ShouldThrowExceptionWhenDeleteFails()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Delete("/relationship/456"),
//                     MockResponse.Http(404)
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 var ex = await Assert.ThrowsAsync<Exception>(async () => await graphClient.DeleteRelationshipAsync(456));
//                 ex.Message.Should().Be("Unable to delete the relationship. The response status was: 404 NotFound");
//             }
//         }
//     }
// }
