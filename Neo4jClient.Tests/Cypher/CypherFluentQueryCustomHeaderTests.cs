using System.Collections.Specialized;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryCustomHeaderTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void SetsMaxExecutionTimeAndCustomHeader_WhenUsingAReturnTypeQuery()
        {
            const string headerName = "HeaderName";
            const string headerValue = "TestHeaderValue";
            var client = Substitute.For<IRawGraphClient>();
            var customHeaders = new NameValueCollection {{headerName, headerValue}};

            var query = new CypherFluentQuery(client)
                .MaxExecutionTime(100)
                .CustomHeaders(customHeaders)
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.Equal(100, query.MaxExecutionTime);
            Assert.Equal(customHeaders, query.CustomHeaders);
        }

        [Fact]
        public void SetsCustomHeader_WhenUsingAReturnTypeQuery()
        {
            const string headerName = "HeaderName";
            const string headerValue = "TestHeaderValue";
            var client = Substitute.For<IRawGraphClient>();
            var customHeaders = new NameValueCollection { { headerName, headerValue } };

            var query = new CypherFluentQuery(client)
                .CustomHeaders(customHeaders)
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.Equal(customHeaders, query.CustomHeaders);
        }

        [Fact]
        public void SetsCustomHeader_WhenUsingANonReturnTypeQuery()
        {
            const string headerName = "HeaderName";
            const string headerValue = "TestHeaderValue";
            var customHeaders = new NameValueCollection { { headerName, headerValue } };

            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .CustomHeaders(customHeaders)
                .Match("n")
                .Set("n.Value = 'value'")
                .Query;

            Assert.Equal(customHeaders, query.CustomHeaders);
        }
    }
}
