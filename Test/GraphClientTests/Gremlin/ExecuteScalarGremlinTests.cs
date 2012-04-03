using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using Neo4jClient.ApiModels.Gremlin;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests.Gremlin
{
    [TestFixture]
    public class ExecuteScalarGremlinTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.ExecuteScalarGremlin("", null);
        }

        [Test]
        public void ShouldReturnScalarValue()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";

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
                    }.AddBody(new GremlinApiQuery(gremlinQueryExpected, null)),
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"1"
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var node = graphClient.ExecuteScalarGremlin(gremlinQueryExpected, null);

            //Assert
            Assert.AreEqual(1, int.Parse(node));
        }
    }
}