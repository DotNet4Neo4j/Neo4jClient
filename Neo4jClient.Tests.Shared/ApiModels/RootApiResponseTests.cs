using FluentAssertions;
using Xunit;
using Neo4jClient.ApiModels;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.ApiModels
{
    
    public class RootApiResponseTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Theory]
        [InlineData("", "0.0")]
        [InlineData("kgrkjkj", "0.0")]
        [InlineData("1.5-82-g7cb21bb1-dirty", "1.5")] //Description = "http://docs.neo4j.org/chunked/snapshot/rest-api-service-root.html")]
        [InlineData("1.5M02", "1.5.0.2")]
        [InlineData("1.8.RC1", "1.8.0.1")]
        [InlineData("1.5.M02", "1.5.0.2")] //Description = "Retrieved via REST call from running 1.5M02 install")]
        [InlineData("1.7", "1.7")] //Description = "http://docs.neo4j.org/chunked/1.7/rest-api-service-root.html")]
        [InlineData("1.7.2", "1.7.2")] //Description = "http://docs.neo4j.org/chunked/1.7.2/rest-api-service-root.html")]
        [InlineData("1.8.M07-1-g09701c5", "1.8.0.7")] //Description = "http://docs.neo4j.org/chunked/1.8.M07/rest-api-service-root.html")]
        [InlineData("1.9.RC1", "1.9.0.1")]
        [InlineData("1.9.RC2", "1.9.0.2")]
        public void Version(string versionString, string expectedResult)
        {
            var response = new RootApiResponse { Neo4jVersion = versionString };
            response.Version.ToString().Should().Be(expectedResult);
        }
    }
}
