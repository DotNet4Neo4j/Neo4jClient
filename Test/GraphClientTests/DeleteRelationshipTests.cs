using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class DeleteRelationshipTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.DeleteRelationship(123);
        }

        [Test]
        public void ShouldDeleteRelationship()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{
                          'batch' : 'http://foo/db/data/batch',
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
                    new RestRequest { Resource = "/relationship/456", Method = Method.DELETE },
                    new HttpResponse { StatusCode = HttpStatusCode.NoContent }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
            graphClient.DeleteRelationship(456);

            Assert.Inconclusive("Not actually asserting that the relationship was deleted");
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Unable to delete the relationship. The response status was: 404 NOT FOUND")]
        public void ShouldThrowApplicationExceptionWhenDeleteFails()
        {
            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<IRestRequest, IHttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{
                          'batch' : 'http://foo/db/data/batch',
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
                    new RestRequest { Resource = "/relationship/456", Method = Method.DELETE },
                    new HttpResponse { StatusCode = HttpStatusCode.NotFound, StatusDescription = "NOT FOUND" }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
            graphClient.DeleteRelationship(456);
        }
    }
}
