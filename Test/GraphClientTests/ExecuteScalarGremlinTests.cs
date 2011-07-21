using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ExecuteScalarGremlinTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.ExecuteScalarGremlin("");
        }

        [Test]
        public void ShouldReturnScalarValue()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";

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
                          'extensions' : {
                            'GremlinPlugin' : {
                              'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                            }
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/ext/GremlinPlugin/graphdb/execute_script",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddParameter("script", gremlinQueryExpected, ParameterType.GetOrPost),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"1"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var node = graphClient.ExecuteScalarGremlin(gremlinQueryExpected);

            //Assert
            Assert.AreEqual(1, int.Parse(node));
        }

        [Test]
        public void ShouldReturnReplaceQueryParameters()
        {
            //Arrange
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
                          'extensions' : {
                            'GremlinPlugin' : {
                              'execute_script' : 'http://foo/db/data/ext/GremlinPlugin/graphdb/execute_script'
                            }
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/ext/GremlinPlugin/graphdb/execute_script",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddParameter("script", "foo baz query", ParameterType.GetOrPost),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"1"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            graphClient.ExecuteScalarGremlin("foo <<bar>> query");

            //Assert
            Assert.Inconclusive("Not actually asserting that the correct query was sent");
        }
    }
}