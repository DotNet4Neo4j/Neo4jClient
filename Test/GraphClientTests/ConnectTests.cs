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
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Received a non-200 HTTP response when connecting to the server. The response status was: 500 Internal Server Error")]
        public void ShouldThrowConnectionExceptionFor500Response()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
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
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions'' : {
                          }
                        }".Replace('\'', '"')
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            Assert.AreEqual("/node", graphClient.RootEndpoints.Node);
            Assert.AreEqual("/index/node", graphClient.RootEndpoints.NodeIndex);
            Assert.AreEqual("/index/relationship", graphClient.RootEndpoints.RelationshipIndex);
            Assert.AreEqual("/node/0", graphClient.RootEndpoints.ReferenceNode);
            Assert.AreEqual("/ext", graphClient.RootEndpoints.ExtensionsInfo);
        }
    }
}