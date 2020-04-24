// using System.Threading.Tasks;
// using Xunit;
//
// namespace Neo4jClient.Tests.GraphClientTests
// {
//     
//     public class DeleteIndexTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         [Fact]
//         public async Task ShouldExecuteSilentlyForSuccessfulDelete()
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Delete("/index/node/MyIndex"),
//                     MockResponse.Http(204)
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 await graphClient.DeleteIndexAsync("MyIndex", IndexFor.Node);
//             }
//         }
//     }
// }
