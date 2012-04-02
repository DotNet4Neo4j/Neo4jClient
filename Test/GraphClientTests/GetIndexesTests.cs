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
    public class GetIndexesTests
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
        public void ShouldReturnHttResponse200()
        {
            //Arrange
            var restRequest = new RestRequest("/index/node", Method.GET)
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
                        Content = @"{
                              'agency24871-clients' : {
                                'to_lower_case' : 'true',
                                'template' : 'http://localhost:5102/db/data/index/node/agency24871-clients/{key}/{value}',
                                '_blueprints:type' : 'MANUAL',
                                'provider' : 'lucene',
                                'type' : 'fulltext'
                              },
                              'agency36681-clients' : {
                                'to_lower_case' : 'true',
                                'template' : 'http://localhost:5102/db/data/index/node/agency36681-clients/{key}/{value}',
                                '_blueprints:type' : 'MANUAL',
                                'provider' : 'lucene',
                                'type' : 'fulltext'
                              }
                            }"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.GetIndexes(IndexFor.Node);

            // Assert
            Assert.Pass("Success.");
        }

        [Test]
        public void ShouldReturnListOfIndexes()
        {
            //Arrange
            var restRequest = new RestRequest("/index/node", Method.GET)
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
                        Content = @"{
                              'agency24871-clients' : {
                                'to_lower_case' : 'true',
                                'template' : 'http://localhost:5102/db/data/index/node/agency24871-clients/{key}/{value}',
                                '_blueprints:type' : 'MANUAL',
                                'provider' : 'lucene',
                                'type' : 'fulltext'
                              },
                              'agency36681-clients' : {
                                'to_lower_case' : 'true',
                                'template' : 'http://localhost:5102/db/data/index/node/agency36681-clients/{key}/{value}',
                                '_blueprints:type' : 'MANUAL',
                                'provider' : 'lucene',
                                'type' : 'fulltext'
                              }
                            }"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var result = graphClient.GetIndexes(IndexFor.Node);

            // Assert
            Assert.IsTrue(result.Count == 2);
        }
    }
}