using System;
using System.Net;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests.Gremlin
{
    [TestFixture]
    public class ExecuteScalarGremlinTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.ExecuteScalarGremlin("", null);
        }

        [Test]
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
                Assert.AreEqual(1, int.Parse(node));
            }
        }
    }
}