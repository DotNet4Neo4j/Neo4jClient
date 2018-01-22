using System.Net;
using FluentAssertions;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class IndexExistsTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Theory]
        [InlineData(IndexFor.Node, "/index/node/MyIndex", HttpStatusCode.OK, true)]
        [InlineData(IndexFor.Node, "/index/node/MyIndex", HttpStatusCode.NotFound, false)]
        [InlineData(IndexFor.Relationship, "/index/relationship/MyIndex", HttpStatusCode.OK, true)]
        [InlineData(IndexFor.Relationship, "/index/relationship/MyIndex", HttpStatusCode.NotFound, false)]
        public void ShouldReturnIfIndexIsFound(
            IndexFor indexFor,
            string indexPath,
            HttpStatusCode httpStatusCode, bool expectedResult)
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(indexPath),
                    MockResponse.Json(httpStatusCode, "")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                graphClient.CheckIndexExists("MyIndex", indexFor).Should().Be(expectedResult);
            }
        }
    }
}
