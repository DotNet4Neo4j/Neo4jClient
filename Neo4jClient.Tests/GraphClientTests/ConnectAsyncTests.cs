using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Neo4jClient.Execution;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ConnectAsyncTests
    {
        [Test]
        public async Task CredentialsPreservedAllTheWayThroughToHttpStack()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Returns(callInfo => { throw new NotImplementedException(); });

            var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"), httpClient);

            try
            {
                await graphClient.ConnectAsync();
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch (NotImplementedException)
            {
                // This will fail because we're not giving it the right
                // HTTP response, but we only care about the request for now
            }
            // ReSharper restore EmptyGeneralCatchClause

            var httpCall = httpClient.ReceivedCalls().Last();
            var httpRequest = (HttpRequestMessage) httpCall.GetArguments()[0];

            StringAssert.AreEqualIgnoringCase("Basic", httpRequest.Headers.Authorization.Scheme);
            StringAssert.AreEqualIgnoringCase("dXNlcm5hbWU6cGFzc3dvcmQ=", httpRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public async Task PassesCorrectStreamHeader_WhenUseStreamIsFalse()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Returns(callInfo => { throw new NotImplementedException(); });

            var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"), httpClient);
            graphClient.ExecutionConfiguration.UseJsonStreaming = false;
            try
            {
                await graphClient.ConnectAsync();
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch (NotImplementedException)
            {
                // This will fail because we're not giving it the right
                // HTTP response, but we only care about the request for now
            }
            // ReSharper restore EmptyGeneralCatchClause

            var httpCall = httpClient.ReceivedCalls().Last();
            var httpRequest = (HttpRequestMessage) httpCall.GetArguments()[0];

            Assert.IsFalse(httpRequest.Headers.Contains("X-Stream"));
        }

        [Test]
        public async Task PassesCorrectStreamHeader_WhenUseStreamIsTrue()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Returns(callInfo => { throw new NotImplementedException(); });

            var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"), httpClient);

            try
            {
                await graphClient.ConnectAsync();
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch (NotImplementedException)
            {
                // This will fail because we're not giving it the right
                // HTTP response, but we only care about the request for now
            }
            // ReSharper restore EmptyGeneralCatchClause

            var httpCall = httpClient.ReceivedCalls().Last();
            var httpRequest = (HttpRequestMessage) httpCall.GetArguments()[0];

            Assert.IsTrue(httpRequest.Headers.Contains("X-Stream"));
            Assert.Contains("true", httpRequest.Headers.GetValues("X-Stream").ToList());
        }


        [Test]
        public void ShouldFireOnCompletedEvenWhenException()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Returns(callInfo => { throw new NotImplementedException(); });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpClient);
            OperationCompletedEventArgs operationCompletedArgs = null;

            graphClient.OperationCompleted += (s, e) => { operationCompletedArgs = e; };

            // act
            Assert.Throws<AggregateException>(() => graphClient.ConnectAsync().Wait());

            Assert.NotNull(operationCompletedArgs);
            Assert.That(operationCompletedArgs.HasException);
            Assert.AreEqual(typeof(NotImplementedException), operationCompletedArgs.Exception.GetType());
        }


        [Test]
        public async Task ShouldParseRootApiResponseFromAuthenticatedConnection()
        {
            using (var testHarness = new RestTestHarness
            {
                {MockRequest.Get(""), MockResponse.NeoRoot()}
            })
            {
                var httpClient = testHarness.GenerateHttpClient("http://foo/db/data");
                var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"), httpClient);
                await graphClient.ConnectAsync();
                Assert.AreEqual("/node", graphClient.RootApiResponse.Node);
            }
        }

        [Test]
        public async Task ShouldSendCustomUserAgent()
        {
            // Arrange
            var httpClient = Substitute.For<IHttpClient>();
            var graphClient = new GraphClient(new Uri("http://localhost"), httpClient);
            var expectedUserAgent = graphClient.ExecutionConfiguration.UserAgent;
            httpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(message =>
                {
                    // Assert
                    Assert.IsTrue(message.Headers.Contains("User-Agent"), "Contains User-Agent header");
                    var userAgent = message.Headers.GetValues("User-Agent").Single();
                    Assert.AreEqual(expectedUserAgent, userAgent, "User-Agent header value is correct");
                }))
                .Returns(ci =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(@"{
                            'cypher' : 'http://foo/db/data/cypher',
                            'batch' : 'http://foo/db/data/batch',
                            'node' : 'http://foo/db/data/node',
                            'node_index' : 'http://foo/db/data/index/node',
                            'relationship_index' : 'http://foo/db/data/index/relationship',
                            'reference_node' : 'http://foo/db/data/node/123',
                            'neo4j_version' : '1.5.M02',
                            'extensions_info' : 'http://foo/db/data/ext',
                            'extensions' : {
                                'GremlinPlugin' : {
                                    'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                                }
                            }
                        }")
                    };
                    var task = new Task<HttpResponseMessage>(() => response);
                    task.Start();
                    return task;
                });

            // Act
            await graphClient.ConnectAsync();
        }
    }
}