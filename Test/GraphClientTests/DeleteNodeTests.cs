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
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
            graphClient.Delete(456, DeleteMode.NodeOnly);

            Assert.Inconclusive("Not actually asserting that the node was deleted");
        }

        [Test]
        public void ShouldDeleteAllRelationshipsFirst()
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
                    new RestRequest { Resource = "/node/456/relationships/all", Method = Method.GET },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"[
                          { 'self': 'http://localhost:7474/db/data/relationship/56',
                            'start': 'http://localhost:7474/db/data/node/123',
                            'end': 'http://localhost:7474/db/data/node/456',
                            'type': 'KNOWS',
                            'properties': 'http://localhost:7474/db/data/relationship/56/properties',
                            'property': 'http://localhost:7474/db/data/relationship/56/properties/{key}',
                            'data': { 'date': 1270559208258 }
                          },
                          { 'self': 'http://localhost:7474/db/data/relationship/78',
                            'start': 'http://localhost:7474/db/data/node/456',
                            'end': 'http://localhost:7474/db/data/node/789',
                            'type': 'KNOWS',
                            'properties': 'http://localhost:7474/db/data/relationship/78/properties',
                            'property': 'http://localhost:7474/db/data/relationship/78/properties/{key}',
                            'data': { 'date': 1270559208258 }
                          }
                        ]".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest { Resource = "/relationship/56", Method = Method.DELETE },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                },
                {
                    new RestRequest { Resource = "/relationship/78", Method = Method.DELETE },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                },
                {
                    new RestRequest { Resource = "/node/456", Method = Method.DELETE },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();
            graphClient.Delete(456, DeleteMode.NodeAndRelationships);

            Assert.Inconclusive("Not actually asserting that the node was deleted");
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
