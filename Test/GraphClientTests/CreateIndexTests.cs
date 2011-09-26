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
    public class CreateIndexTests
    {
        const string RootResponse = @"{
                          'batch' : 'http://foo/db/data/batch',
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
        public void ShouldReturnHttpResponse201WhenCreatingAnIndexOfTypeFullText()
        {
            //Arrange
            var indexConfiguration = new IndexConfiguration
                {
                    Provider = IndexProvider.lucene,
                    Type = IndexType.fulltext

                };

            var createIndexApiRequest = new
            {
                name = "foo",
                config = indexConfiguration
            };

            var restRequest = new RestRequest("/index/node", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };
            restRequest.AddBody(createIndexApiRequest);

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
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.CreateIndex("foo", indexConfiguration, IndexFor.Node);

            // Assert
            Assert.Pass("Success.");
        }

        [Test]
        public void ShouldReturnHttpResponse201WhenCreatingAnIndexOfTypeExact()
        {
            //Arrange
            var indexConfiguration = new IndexConfiguration
            {
                Provider = IndexProvider.lucene,
                Type = IndexType.exact

            };

            var createIndexApiRequest = new
            {
                name = "foo",
                config = indexConfiguration
            };

            var restRequest = new RestRequest("/index/node", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };
            restRequest.AddBody(createIndexApiRequest);

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
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.CreateIndex("foo", indexConfiguration, IndexFor.Node);

            // Assert
            Assert.Pass("Success.");
        }

        [Test]
        public void ShouldReturnHttpResponse201WhenCreatingAnIndexForRelationship()
        {
            //Arrange
            var indexConfiguration = new IndexConfiguration
            {
                Provider = IndexProvider.lucene,
                Type = IndexType.exact

            };

            var createIndexApiRequest = new
            {
                name = "foo",
                config = indexConfiguration
            };

            var restRequest = new RestRequest("/index/relationship", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };
            restRequest.AddBody(createIndexApiRequest);

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
                        StatusCode = HttpStatusCode.Created,
                        ContentType = "application/json",
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.CreateIndex("foo", indexConfiguration, IndexFor.Relationship);

            // Assert
            Assert.Pass("Success.");
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void ShouldThrowApplicationExceptionIfHttpCodeIsNot201()
        {
            //Arrange
            var indexConfiguration = new IndexConfiguration
            {
                Provider = IndexProvider.lucene,
                Type = IndexType.exact

            };

            var createIndexApiRequest = new
            {
                name = "foo",
                config = indexConfiguration
            };

            var restRequest = new RestRequest("/index/relationship", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };
            restRequest.AddBody(createIndexApiRequest);

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
                        StatusCode = HttpStatusCode.ServiceUnavailable,
                        ContentType = "application/json",
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.CreateIndex("foo", indexConfiguration, IndexFor.Relationship);
        }
    }
}