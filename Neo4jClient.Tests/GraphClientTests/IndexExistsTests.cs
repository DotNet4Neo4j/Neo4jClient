using System.Net;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class IndexExistsTests
    {
        [Test]
        [TestCase(IndexFor.Node, "/index/node/MyIndex", HttpStatusCode.OK, ExpectedResult = true)]
        [TestCase(IndexFor.Node, "/index/node/MyIndex", HttpStatusCode.NotFound, ExpectedResult = false)]
        [TestCase(IndexFor.Relationship, "/index/relationship/MyIndex", HttpStatusCode.OK, ExpectedResult = true)]
        [TestCase(IndexFor.Relationship, "/index/relationship/MyIndex", HttpStatusCode.NotFound, ExpectedResult = false)]
        public bool ShouldReturnIfIndexIsFound(
            IndexFor indexFor,
            string indexPath,
            HttpStatusCode httpStatusCode)
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
                return graphClient.CheckIndexExists("MyIndex", indexFor);
            }
        }
    }
}
