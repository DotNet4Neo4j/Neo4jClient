using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class DeleteNodeTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Delete(123);
        }

        [Test]
        public void ShouldDeleteNode()
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
                },
                {
                    new RestRequest { Resource = "/node/456", Method = Method.DELETE },
                    new HttpResponse { StatusCode = HttpStatusCode.NoContent }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
            graphClient.Delete(456);

            Assert.Inconclusive("Not actually asserting that the node was deleted");
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Unable to delete the node. The node may still have relationships. The response status was: 409 CONFLICT")]
        public void ShouldThrowApplicationExceptionWhenDeleteFails()
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
                },
                {
                    new RestRequest { Resource = "/node/456", Method = Method.DELETE },
                    new HttpResponse { StatusCode = HttpStatusCode.Conflict, StatusDescription = "CONFLICT" }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
            graphClient.Delete(456);
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }
    }
}
