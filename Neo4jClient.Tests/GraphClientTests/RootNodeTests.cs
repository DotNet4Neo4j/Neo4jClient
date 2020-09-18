// using System.Threading.Tasks;
// using Xunit;
//
// namespace Neo4jClient.Tests.GraphClientTests
// {
//     
//     public class RootNodeTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         [Fact]
//         public async Task RootNodeShouldHaveReferenceBackToClient()
//         {
//             using (var testHarness = new RestTestHarness())
//             {
//                 var client = await testHarness.CreateAndConnectGraphClient();
//                 var rootNode = client.RootNode;
//                 Assert.Equal(client, ((IAttachedReference) rootNode).Client);
//             }
//         }
//     }
// }
