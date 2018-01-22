using System;
using System.Net;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.GraphClientTests.Gremlin
{
    
    public class ExecuteScalarGremlinTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            Assert.Throws<InvalidOperationException>(() => client.ExecuteScalarGremlin("", null));
        }

        [Fact]
        public void ShouldReturnScalarValue()
        {
            //Arrange
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson(
                        "/ext/GremlinPlugin/graphdb/execute_script",
                        @"{ 'script': 'foo bar query', 'params': {} }"),
                    MockResponse.Json(HttpStatusCode.OK, @"1")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var node = graphClient.ExecuteScalarGremlin("foo bar query", null);

                //Assert
                Assert.Equal(1, int.Parse(node));
            }
        }

        [Fact]
        public void ShouldFailGracefullyWhenGremlinIsNotAvailable()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot20()
                }
            })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                var ex = Assert.Throws<Exception>(
                    () => graphClient.ExecuteScalarGremlin("foo bar query", null));
                Assert.Equal(GraphClient.GremlinPluginUnavailable, ex.Message);
            }
        }
    }
}