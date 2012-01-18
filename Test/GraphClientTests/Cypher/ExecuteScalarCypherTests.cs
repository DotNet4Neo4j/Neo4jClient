using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels.Cypher;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class ExecuteScalarCypherTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.ExecuteScalarCypher("", null);
        }

        [Test]
        public void ShouldReturnScalarValue()
        {
            //Arrange
            const string cypherExpectedQuery = "start myNode=node({foo}) return myNode";

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{
                          'cypher' : 'http://foo/db/data/cypher',
                          'batch' : 'http://foo/db/data/batch',
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions' : {
                            'CypherPlugin' : {
                              'execute_script' : 'http://foo/db/data/ext/CypherPlugin/graphdb/execute_script'
                            }
                          }
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/cypher",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new CypherApiQuery(cypherExpectedQuery, null)),
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
            var node = graphClient.ExecuteScalarCypher(cypherExpectedQuery, null);

            //Assert
            Assert.AreEqual(1, int.Parse(node));
        }
    }
}