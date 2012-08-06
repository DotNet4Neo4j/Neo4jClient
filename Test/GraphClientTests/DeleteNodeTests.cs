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
            client.Delete(123, DeleteMode.NodeOnly);
        }

        [Test]
        public void ShouldDeleteNodeOnly()
        {
            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
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
                },
                {
                    new NeoHttpRequest { Resource = "/node/456", Method = Method.DELETE },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                }
            };

            var graphClient = testHarness.CreateAndConnectGraphClient();
            graphClient.Delete(456, DeleteMode.NodeOnly);

            testHarness.AssertAllRequestsWereReceived();
        }

        [Test]
        public void ShouldDeleteAllRelationshipsFirst()
        {
            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
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
                },
                {
                    new NeoHttpRequest { Resource = "/node/456/relationships/all", Method = Method.GET },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"[
                          { 'self': 'http://foo/db/data/relationship/56',
                            'start': 'http://foo/db/data/node/123',
                            'end': 'http://foo/db/data/node/456',
                            'type': 'KNOWS',
                            'properties': 'http://foo/db/data/relationship/56/properties',
                            'property': 'http://foo/db/data/relationship/56/properties/{key}',
                            'data': { 'date': 1270559208258 }
                          },
                          { 'self': 'http://foo/db/data/relationship/78',
                            'start': 'http://foo/db/data/node/456',
                            'end': 'http://foo/db/data/node/789',
                            'type': 'KNOWS',
                            'properties': 'http://foo/db/data/relationship/78/properties',
                            'property': 'http://foo/db/data/relationship/78/properties/{key}',
                            'data': { 'date': 1270559208258 }
                          }
                        ]".Replace('\'', '"')
                    }
                },
                {
                    new NeoHttpRequest { Resource = "/relationship/56", Method = Method.DELETE },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                },
                {
                    new NeoHttpRequest { Resource = "/relationship/78", Method = Method.DELETE },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                },
                {
                    new NeoHttpRequest { Resource = "/node/456", Method = Method.DELETE },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                }
            };

            var graphClient = testHarness.CreateAndConnectGraphClient();
            graphClient.Delete(456, DeleteMode.NodeAndRelationships);

            testHarness.AssertAllRequestsWereReceived();
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Unable to delete the node. The node may still have relationships. The response status was: 409 CONFLICT")]
        public void ShouldThrowApplicationExceptionWhenDeleteFails()
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
                },
                {
                    new RestRequest { Resource = "/node/456", Method = Method.DELETE },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.Conflict, StatusDescription = "CONFLICT" }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
            graphClient.Delete(456, DeleteMode.NodeOnly);
        }
    }
}
