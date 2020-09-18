// using System.Net;
// using System.Threading.Tasks;
// using FluentAssertions;
// using Xunit;
//
// namespace Neo4jClient.Tests.GraphClientTests
// {
//     
//     public class IndexExistsTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         [Theory]
//         [InlineData(IndexFor.Node, "/index/node/MyIndex", HttpStatusCode.OK, true)]
//         [InlineData(IndexFor.Node, "/index/node/MyIndex", HttpStatusCode.NotFound, false)]
//         [InlineData(IndexFor.Relationship, "/index/relationship/MyIndex", HttpStatusCode.OK, true)]
//         [InlineData(IndexFor.Relationship, "/index/relationship/MyIndex", HttpStatusCode.NotFound, false)]
//         public async Task ShouldReturnIfIndexIsFound(
//             IndexFor indexFor,
//             string indexPath,
//             HttpStatusCode httpStatusCode, bool expectedResult)
//         {
//             using (var testHarness = new RestTestHarness
//             {
//                 {
//                     MockRequest.Get(indexPath),
//                     MockResponse.Json(httpStatusCode, "")
//                 }
//             })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//                 (await graphClient.CheckIndexExistsAsync("MyIndex", indexFor)).Should().Be(expectedResult);
//             }
//         }
//     }
// }
