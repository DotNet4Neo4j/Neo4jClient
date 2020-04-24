using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class ConnectTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task ShouldThrowConnectionExceptionFor500Response()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.Http(500)
                }
            })
            {
                var ex = await Assert.ThrowsAsync<Exception>(async () => await testHarness.CreateAndConnectGraphClient());
                Assert.Equal("Received an unexpected HTTP status when executing the request.\r\n\r\nThe response status was: 500 InternalServerError", ex.Message);
            }
        }

        [Fact]
        public async Task ShouldRetrieveApiEndpoints()
        {
            using (var testHarness = new RestTestHarness())
            {
                var graphClient = (GraphClient)await testHarness.CreateAndConnectGraphClient();
                Assert.Equal("/node", graphClient.RootApiResponse.Node);
                Assert.Equal("/index/node", graphClient.RootApiResponse.NodeIndex);
                Assert.Equal("/index/relationship", graphClient.RootApiResponse.RelationshipIndex);
                Assert.Equal("http://foo/db/data/node/123", graphClient.RootApiResponse.ReferenceNode);
                Assert.Equal("/ext", graphClient.RootApiResponse.ExtensionsInfo);
            }
        }

        [Fact]
        public async Task ShouldParse15M02Version()
        {
            RootApiResponse rar = new RootApiResponse
            {
                Neo4jVersion = "1.5.M02"
            };
            
            Assert.Equal("1.5.0.2", rar.Version.ToString());
        }

        [Fact]
        public async Task ShouldReturnCypher19CapabilitiesForPre20Version()
        {
            using (var testHarness = new RestTestHarness())
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();
                Assert.Equal(CypherCapabilities.Cypher19, graphClient.CypherCapabilities);
            }
        }

        [Fact]
        public async Task ShouldSetCypher22CapabilitiesForPost22Version()
        {
            using (var testHarness = new RestTestHarness())
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient(RestTestHarness.Neo4jVersion.Neo22);
                Assert.Equal(CypherCapabilities.Cypher22, graphClient.CypherCapabilities);
            }
        }

        [Fact]
        public async Task ShouldReturnCypher19CapabilitiesForVersion20()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot20()
                }
            })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();
                Assert.Equal(CypherCapabilities.Cypher20, graphClient.CypherCapabilities);
            }
        }

        [Fact]
        public void UserInfoPreservedInRootUri()
        {
            var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"));

            Assert.Equal(graphClient.RootUri.OriginalString, "http://username:password@foo/db/data");
        }

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
            var httpRequest = (HttpRequestMessage)httpCall.GetArguments()[0];

            Assert.True(httpRequest.Headers.Contains("X-Stream"));
            Assert.Contains("true", httpRequest.Headers.GetValues("X-Stream").ToList());
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
            var httpRequest = (HttpRequestMessage)httpCall.GetArguments()[0];

            Assert.False(httpRequest.Headers.Contains("X-Stream"));
        }


        [Fact]
        public async Task ShouldParseRootApiResponseFromAuthenticatedConnection()
        {
            using (var testHarness = new RestTestHarness()
            {
                { MockRequest.Get(""), MockResponse.NeoRoot20() }
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
                .Returns(ci => {
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
        public void ShouldFormatUserAgentCorrectly()
        {
            var graphClient = new GraphClient(new Uri("http://localhost"));
            var userAgent = graphClient.ExecutionConfiguration.UserAgent;
            Assert.True(Regex.IsMatch(userAgent, @"Neo4jClient/\d+\.\d+\.\d+\.\d+"), "User agent should be in format Neo4jClient/1.2.3.4");
        }

        [Fact]
        public void ShouldFormatAuthorisationHeaderCorrectly()
        {
            const string username = "user";
            const string password = "password";
            var expectedHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));

            var graphClient = new GraphClient(new Uri("http://localhost"), username, password);
            var httpClient = (HttpClientWrapper) graphClient.ExecutionConfiguration.HttpClient;

            Assert.Equal(expectedHeader, httpClient.AuthenticationHeaderValue.Parameter);
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

            graphClient.OperationCompleted+= (s, e) =>
            {
                operationCompletedArgs = e;
            };

            // act
            await Assert.ThrowsAsync<NotImplementedException>(async () => await graphClient.ConnectAsync());

            Assert.NotNull(operationCompletedArgs);
            Assert.True(operationCompletedArgs.HasException);
            Assert.Equal(typeof(NotImplementedException), operationCompletedArgs.Exception.GetType());
        }

    }
}
