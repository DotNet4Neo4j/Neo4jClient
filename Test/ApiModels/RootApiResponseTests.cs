using NUnit.Framework;
using Neo4jClient.ApiModels;

namespace Neo4jClient.Test.ApiModels
{
    [TestFixture]
    public class RootApiResponseTests
    {
        [Test]
        [TestCase("", Result = "0.0")]
        [TestCase("kgrkjkj", Result = "0.0")]
        [TestCase("1.5-82-g7cb21bb1-dirty", Result = "1.5", Description = "http://docs.neo4j.org/chunked/snapshot/rest-api-service-root.html")]
        [TestCase("1.5M02", Result = "1.5.0.2")]
        [TestCase("1.8.RC1", Result = "1.8.0.8")]
        [TestCase("1.5.M02", Result = "1.5.0.2", Description = "Retrieved via REST call from running 1.5M02 install")]
        public string Version(string versionString)
        {
            var response = new RootApiResponse { neo4j_version = versionString };
            return response.Version.ToString();
        }
    }
}
