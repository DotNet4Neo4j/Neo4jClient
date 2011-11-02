using NUnit.Framework;
using Neo4jClient.ApiModels;

namespace Neo4jClient.Test.ApiModels
{
    [TestFixture]
    public class RootApiResponseTests
    {
        [Test]
        [TestCase("", Result = "0.0")]
        [TestCase("1.5-82-g7cb21bb1-dirty", Result = "1.5.82", Description = "http://docs.neo4j.org/chunked/snapshot/rest-api-service-root.html")]
        public string Version(string versionString)
        {
            var response = new RootApiResponse { VersionString = versionString };
            return response.Version.ToString();
        }
    }
}
