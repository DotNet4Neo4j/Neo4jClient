using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class CreateIndexTests
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
        public void ShoudlReturnHttpResponse201WhenCreatingAnIndexOfTypeFullText()
        {
            //Arrange
            var indexConfiguration = new IndexConfiguration
                {
                    Provider = IndexProvider.Lucene,
                    Type = IndexType.FullText

                };

            var createIndexApiRequest = new
            {
                name = "foo",
                config = indexConfiguration
            };

            var restRequest = new RestRequest
            {
                Resource = "/index/node",
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };
            restRequest.AddBody(restRequest.JsonSerializer.Serialize(createIndexApiRequest));

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
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.CreateIndex("foo", indexConfiguration, IndexFor.Node);

            // Assert
            Assert.Pass("Method executed successfully.");
        }

        [Test]
        public void ShoudlReturnHttpResponse201WhenCreatingAnIndexOfTypeExact()
        {
            //Arrange
            var indexConfiguration = new IndexConfiguration
            {
                Provider = IndexProvider.Lucene,
                Type = IndexType.Exact

            };

            var createIndexApiRequest = new
            {
                name = "foo",
                config = indexConfiguration
            };

            var restRequest = new RestRequest
            {
                Resource = "/index/node",
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };
            restRequest.AddBody(restRequest.JsonSerializer.Serialize(createIndexApiRequest));

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
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.CreateIndex("foo", indexConfiguration, IndexFor.Node);

            // Assert
            Assert.Pass("Method executed successfully.");
        }

        [Test]
        public void ShoudlReturnHttpResponse201WhenCreatingAnIndexForRelationship()
        {
            //Arrange
            var indexConfiguration = new IndexConfiguration
            {
                Provider = IndexProvider.Lucene,
                Type = IndexType.Exact

            };

            var createIndexApiRequest = new
            {
                name = "foo",
                config = indexConfiguration
            };

            var restRequest = new RestRequest
            {
                Resource = "/index/relationship",
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };
            restRequest.AddBody(restRequest.JsonSerializer.Serialize(createIndexApiRequest));

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
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.CreateIndex("foo", indexConfiguration, IndexFor.Relationship);

            // Assert
            Assert.Pass("Method executed successfully.");
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ShoudlThrowNotSupportedExceptionIfHttpCodeIsNot201()
        {
            //Arrange
            var indexConfiguration = new IndexConfiguration
            {
                Provider = IndexProvider.Lucene,
                Type = IndexType.Exact

            };

            var createIndexApiRequest = new
            {
                name = "foo",
                config = indexConfiguration
            };

            var restRequest = new RestRequest
            {
                Resource = "/index/relationship",
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };
            restRequest.AddBody(restRequest.JsonSerializer.Serialize(createIndexApiRequest));

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