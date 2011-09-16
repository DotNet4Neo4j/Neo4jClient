using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Neo4jClient.Serializer;
using Newtonsoft.Json;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class IndexExistsTests
    {
        const string RootResponse = @"{
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                            'GremlinPlugin' : {
                              'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                            }
                          }
                        }";

        [Test]
        public void ShouldReturnTrueIfIndexIsFound()
        {
            //Arrange
            var restRequest = new RestRequest("/index/node/MyIndex", Method.GET)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = RootResponse.Replace('\'', '"')
                    }
                },
                {
                    restRequest,
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @""
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var response = graphClient.CheckIndexExists("MyIndex",IndexFor.Node);

            // Assert
            Assert.IsTrue(response);
        }

        [Test]
        public void ShouldReturnFalseIfIndexNotFound()
        {
            //Arrange
            var restRequest = new RestRequest("/index/node/MyIndex", Method.GET)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = RootResponse.Replace('\'', '"')
                    }
                },
                {
                    restRequest,
                    new HttpResponse {
                        StatusCode = HttpStatusCode.NotFound
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var response = graphClient.CheckIndexExists("MyIndex", IndexFor.Node);

            // Assert
            Assert.IsFalse(response);
        }
    }
}