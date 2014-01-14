using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ConnectTests
    {
        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Received an unexpected HTTP status when executing the request.\r\n\r\nThe response status was: 500 InternalServerError")]
        public void ShouldThrowConnectionExceptionFor500Response()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.Http(500)
                }
            })
            {
                testHarness.CreateAndConnectGraphClient();
            }
        }

        [Test]
        public void ShouldRetrieveApiEndpoints()
        {
            using (var testHarness = new RestTestHarness())
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                Assert.AreEqual("/node", graphClient.RootApiResponse.Node);
                Assert.AreEqual("/index/node", graphClient.RootApiResponse.NodeIndex);
                Assert.AreEqual("/index/relationship", graphClient.RootApiResponse.RelationshipIndex);
                Assert.AreEqual("http://foo/db/data/node/123", graphClient.RootApiResponse.ReferenceNode);
                Assert.AreEqual("/ext", graphClient.RootApiResponse.ExtensionsInfo);
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "The graph client is not connected to the server. Call the Connect method first.")]
        public void RootNode_ShouldThrowInvalidOperationException_WhenNotConnectedYet()
        {
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), null);
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
            graphClient.RootNode.ToString();
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void RootNode_ShouldReturnReferenceNode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                Assert.IsNotNull(graphClient.RootNode);
                Assert.AreEqual(123, graphClient.RootNode.Id);
            }
        }

        [Test]
        public void RootNode_ShouldReturnNullReferenceNode_WhenNoReferenceNodeDefined()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.Json(HttpStatusCode.OK, @"{
                        'batch' : 'http://foo/db/data/batch',
                        'node' : 'http://foo/db/data/node',
                        'node_index' : 'http://foo/db/data/index/node',
                        'relationship_index' : 'http://foo/db/data/index/relationship',
                        'extensions_info' : 'http://foo/db/data/ext',
                        'extensions' : {
                        }
                    }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                Assert.IsNull(graphClient.RootNode);
            }
        }

        [Test]
        public void ShouldParse15M02Version()
        {
            using (var testHarness = new RestTestHarness())
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                Assert.AreEqual("1.5.0.2", graphClient.RootApiResponse.Version.ToString());
            }
        }

        [Test]
        public void ShouldReturnCypher19CapabilitiesForPre20Version()
        {
            using (var testHarness = new RestTestHarness())
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                Assert.AreEqual(CypherCapabilities.Cypher19, graphClient.CypherCapabilities);
            }
        }

        [Test]
        public void ShouldReturnCypher19CapabilitiesForVersion20()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot20()
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                Assert.AreEqual(CypherCapabilities.Cypher20, graphClient.CypherCapabilities);
            }
        }

        [Test]
        public void UserInfoPreservedInRootUri()
        {
            var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"));

            Assert.That(graphClient.RootUri.OriginalString, Is.EqualTo("http://username:password@foo/db/data"));
        }

        [Test]
        public void CredentialsPreservedAllTheWayThroughToHttpStack()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient
                .SendAsync(Arg.Any<HttpRequestMessage>())
                .Returns(callInfo => { throw new NotImplementedException(); });

            var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"), httpClient);

            try
            {
                graphClient.Connect();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (NotImplementedException)
            {
                // This will fail because we're not giving it the right
                // HTTP response, but we only care about the request for now
            }
            // ReSharper restore EmptyGeneralCatchClause

            var httpCall = httpClient.ReceivedCalls().First();
            var httpRequest = (HttpRequestMessage) httpCall.GetArguments()[0];

            StringAssert.AreEqualIgnoringCase("Basic", httpRequest.Headers.Authorization.Scheme);
            StringAssert.AreEqualIgnoringCase("dXNlcm5hbWU6cGFzc3dvcmQ=", httpRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void ShouldParseRootApiResponseFromAuthenticatedConnection()
        {
            using (var testHarness = new RestTestHarness()
            {
                { MockRequest.Get(""), MockResponse.NeoRoot() }
            })
            {
                var httpClient = testHarness.GenerateHttpClient("http://foo/db/data");
                var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"), httpClient);
                graphClient.Connect();
                Assert.AreEqual("/node", graphClient.RootApiResponse.Node);
            }
        }

        [Test]
        public void ShouldSendCustomUserAgent()
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
            graphClient.Connect();
        }

        [Test]
        public void ShouldFormatUserAgentCorrectly()
        {
            var graphClient = new GraphClient(new Uri("http://localhost"));
            var userAgent = graphClient.ExecutionConfiguration.UserAgent;
            Assert.IsTrue(Regex.IsMatch(userAgent, @"Neo4jClient/\d+\.\d+\.\d+\.\d+"), "User agent should be in format Neo4jClient/1.2.3.4");
        }
    }
}
