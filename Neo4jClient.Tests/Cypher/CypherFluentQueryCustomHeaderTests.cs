using System.Collections.Specialized;
using Neo4jClient.Cypher;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryCustomHeaderTests
    {
        [Test]
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

            Assert.AreEqual(100, query.MaxExecutionTime);
            Assert.AreEqual(customHeaders, query.CustomHeaders);
        }

        [Test]
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

            Assert.AreEqual(customHeaders, query.CustomHeaders);
        }

        [Test]
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

            Assert.AreEqual(customHeaders, query.CustomHeaders);
        }
    }
}
