using System;
using System.Collections.Generic;
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
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        StatusDescription = "Internal Server Error"
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
        }

        [Test]
        public void ShouldRetrieveApiEndpoints()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            Assert.AreEqual("/node", graphClient.RootApiResponse.Node);
            Assert.AreEqual("/index/node", graphClient.RootApiResponse.NodeIndex);
            Assert.AreEqual("/index/relationship", graphClient.RootApiResponse.RelationshipIndex);
            Assert.AreEqual("http://foo/db/data/node/0", graphClient.RootApiResponse.ReferenceNode);
            Assert.AreEqual("/ext", graphClient.RootApiResponse.ExtensionsInfo);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "The graph client is not connected to the server. Call the Connect method first.")]
        public void RootNode_ShouldThrowInvalidOperationException_WhenNotConnectedYet()
        {
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), null);
            graphClient.RootNode.ToString();
        }

        [Test]
        public void RootNode_ShouldReturnReferenceNode()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/123',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            Assert.IsNotNull(graphClient.RootNode);
            Assert.AreEqual(123, graphClient.RootNode.Id);
        }

        [Test]
        public void RootNode_ShouldReturnNullReferenceNode_WhenNoReferenceNodeDefined()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            Assert.IsNull(graphClient.RootNode);
        }

        [Test]
        public void ShouldParse15M02Version()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'neo4j_version' : '1.5.M02',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            Assert.AreEqual("1.5.0.2", graphClient.RootApiResponse.Version.ToString());
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Received an unexpected HTTP status when executing the request.\r\n\r\nThe response status was: 401 Unauthorized")]
        public void DisableSupportForNeo4jOnHerokuWhenRequiredThrow401UnAuthroized()
        {
            const string httpFooDbData = "http://foo/db/data";

            var httpFactory = MockHttpFactory.Generate(httpFooDbData, new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        StatusDescription = "Unauthorized"
                    }
                }
            });

            var graphClient = new GraphClient(new Uri(httpFooDbData), httpFactory) { EnableSupportForNeo4jOnHeroku = false };

            graphClient.Connect();
        }

        [Test]
        public void DisableSupportForNeo4jOnHerokuShouldNotChangeResource()
        {
            const string httpFooDbData = "http://foo/db/data";

            var httpFactory = MockHttpFactory.Generate(httpFooDbData, new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'neo4j_version' : '1.5.M02',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri(httpFooDbData), httpFactory) { EnableSupportForNeo4jOnHeroku = false };

            graphClient.Connect();

            Assert.Pass("The constructed URL matched {0} and did not have a trailing slash", httpFooDbData);
        }

        [Test]
        public void EnableSupportForNeo4jOnHerokuShouldChangeResource()
        {
            const string httpFooDbData = "http://foo/db/data/";

            var httpFactory = MockHttpFactory.Generate(httpFooDbData, new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "", Method = Method.GET },
                    new NeoHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'neo4j_version' : '1.5.M02',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                          }
                        }".Replace('\'', '"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri(httpFooDbData), httpFactory) { EnableSupportForNeo4jOnHeroku = true };

            graphClient.Connect();

            Assert.Pass("The constructed URL matched {0} and did have a trailing slash", httpFooDbData);
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