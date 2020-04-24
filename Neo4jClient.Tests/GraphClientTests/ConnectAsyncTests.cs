using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Neo4jClient.Execution;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class ConnectAsyncTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task CredentialsPreservedAllTheWayThroughToHttpStack()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Throws(new NotImplementedException());

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

            Assert.Equal("Basic", httpRequest.Headers.Authorization.Scheme, StringComparer.OrdinalIgnoreCase);
            Assert.Equal("dXNlcm5hbWU6cGFzc3dvcmQ=", httpRequest.Headers.Authorization.Parameter, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PassesCorrectStreamHeader_WhenUseStreamIsFalse()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Throws(new NotImplementedException());

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

            Assert.False(httpRequest.Headers.Contains("X-Stream"));
        }

        [Fact]
        public async Task PassesCorrectStreamHeader_WhenUseStreamIsTrue()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Throws(new NotImplementedException());

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

            Assert.True(httpRequest.Headers.Contains("X-Stream"));
            Assert.Contains("true", httpRequest.Headers.GetValues("X-Stream").ToList());
        }


        [Fact]
        public async Task ShouldFireOnCompletedEvenWhenException()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Throws(new NotImplementedException());

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpClient);
            OperationCompletedEventArgs operationCompletedArgs = null;

            graphClient.OperationCompleted += (s, e) => { operationCompletedArgs = e; };

            // act
            Assert.Throws<AggregateException>(() => graphClient.ConnectAsync().Wait());

            Assert.NotNull(operationCompletedArgs);
            Assert.True(operationCompletedArgs.HasException);
            Assert.Equal(typeof(NotImplementedException), operationCompletedArgs.Exception.GetType());
        }


        [Fact]
        public async Task ShouldParseRootApiResponseFromAuthenticatedConnection()
        {
            using (var testHarness = new RestTestHarness
            {
                {MockRequest.Get(""), MockResponse.NeoRoot20()}
            })
            {
                var httpClient = testHarness.GenerateHttpClient("http://foo/db/data");
                var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"), httpClient);
                await graphClient.ConnectAsync();
                Assert.Equal("/node", graphClient.RootApiResponse.Node);
            }
        }

        [Fact]
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
                    Assert.True(message.Headers.Contains("User-Agent"), "Contains User-Agent header");
                    var userAgent = message.Headers.GetValues("User-Agent").Single();
                    Assert.Equal(expectedUserAgent, userAgent);
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

        [Fact]
        public async Task ShouldParseRootApiResponseFromA4xServer()
        {
            // Arrange
            var httpClient = Substitute.For<IHttpClient>();
            var graphClient = new GraphClient(new Uri("http://localhost:7474/"), httpClient);

            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Returns(ci =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(@"{
                            'bolt_direct': 'neo4j://localhost:7687/bolt',
                            'bolt_routing': 'neo4j://localhost:7687/route',
                            'cluster': 'http://localhost:7474/db/{databaseName}/cluster',
                            'transaction': 'http://localhost:7474/db/{databaseName}/tx',
                            'neo4j_version': '4.0.0',
                            'neo4j_edition': 'enterprise'
                        }")
                    };
                    var task = new Task<HttpResponseMessage>(() => response);
                    task.Start();
                    return task;
                });

            // Act
            await graphClient.ConnectAsync();

            graphClient.RootApiResponse.BoltDirect.Should().Be("neo4j://localhost:7687/bolt");
            graphClient.RootApiResponse.BoltRouting.Should().Be("neo4j://localhost:7687/route");

            graphClient.RootApiResponse.Cluster.Should().Be("/db/{databaseName}/cluster");
            graphClient.RootApiResponse.Transaction.Should().Be("/db/{databaseName}/tx");
            graphClient.RootApiResponse.Neo4jVersion.Should().Be("4.0.0");
            graphClient.RootApiResponse.Neo4jEdition.Should().Be("enterprise");
        }
    }
}