using System;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ConnectTests
    {
        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Received an unexpected HTTP status when executing the request.\r\n\r\nThe response status was: 500 Internal Server Error")]
        public void ShouldThrowConnectionExceptionFor500Response()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.InternalServerError()
                }
            })
            {
                testHarness.CreateAndConnectGraphClient();
            }
        }

        [Test]
        public void ShouldRetrieveApiEndpoints()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot()
                }
            })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                Assert.AreEqual("/node", graphClient.RootApiResponse.Node);
                Assert.AreEqual("/index/node", graphClient.RootApiResponse.NodeIndex);
                Assert.AreEqual("/index/relationship", graphClient.RootApiResponse.RelationshipIndex);
                Assert.AreEqual("http://foo/db/data/node/0", graphClient.RootApiResponse.ReferenceNode);
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
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.Json(HttpStatusCode.OK, @"{
                        'batch' : 'http://foo/db/data/batch',
                        'node' : 'http://foo/db/data/node',
                        'node_index' : 'http://foo/db/data/index/node',
                        'relationship_index' : 'http://foo/db/data/index/relationship',
                        'reference_node' : 'http://foo/db/data/node/123',
                        'neo4j_version' : '1.5.M02',
                        'extensions_info' : 'http://foo/db/data/ext',
                        'extensions' : {
                        }
                    }")
                }
            })
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
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot()
                }
            })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                Assert.AreEqual("1.5.0.2", graphClient.RootApiResponse.Version.ToString());
            }
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Received an unexpected HTTP status when executing the request.\r\n\r\nThe response status was: 401 Unauthorized")]
        public void DisableSupportForNeo4JOnHerokuWhenRequiredThrow401UnAuthorized()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.Unauthorized()
                }
            })
            {
                var graphClient = testHarness.CreateGraphClient();
                graphClient.EnableSupportForNeo4jOnHeroku = false;
                graphClient.Connect();
            }
        }

        [Test]
        public void DisableSupportForNeo4JOnHerokuShouldNotChangeResource()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot()
                }
            })
            {
                var graphClient = testHarness.CreateGraphClient();
                graphClient.EnableSupportForNeo4jOnHeroku = false;
                graphClient.Connect();
            }
        }

        [Test]
        public void EnableSupportForNeo4JOnHerokuShouldChangeResourceToTrailingSlash()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/"),
                    MockResponse.NeoRoot()
                }
            })
            {
                var graphClient = testHarness.CreateGraphClient();
                graphClient.EnableSupportForNeo4jOnHeroku = true;
                graphClient.Connect();
            }
        }

        [Test]
        public void BasicAuthenticatorNotUsedWhenNoUserInfoSupplied()
        {
            var graphClient = new GraphClient(new Uri("http://foo/db/data"));

            Assert.IsNull(graphClient.Authenticator);
        }

        [Test]
        public void BasicAuthenticatorUsedWhenUserInfoSupplied()
        {
            var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"));

            Assert.That(graphClient.Authenticator, Is.TypeOf<HttpBasicAuthenticator>());
        }

        [Test]
        public void UserInfoRemovedFromRootUri()
        {
            var graphClient = new GraphClient(new Uri("http://username:password@foo/db/data"));

            Assert.That(graphClient.RootUri.OriginalString, Is.EqualTo("http://foo/db/data"));
        }
    }
}