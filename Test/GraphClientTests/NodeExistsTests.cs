using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class NodeExistsTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Get<object>(123);
        }

        [Test]
        public void ShouldReturnACountOf1()
        {
            //Arrange
            const string relationshipType = "GENCIES_CATEGORY";
            const string gremlinQueryExpected = @"g.V.outE[[label:'" + relationshipType + "']].inV[['Key':'foo']].count()";

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

            const string agencyKey = "foo";
            var queryParamaters =  new NameValueCollection
            {
                {"<<RelationshipType>>", relationshipType},
                {"<<AgencyKey>>", agencyKey}
            };

            //Act
            const string gremlinQuery = @"g.V.outE[[label:'<<RelationshipType>>']].inV[['Key':'<<AgencyKey>>']].count()";
            var node = graphClient.ExecuteScalarGremlin(gremlinQuery, queryParamaters);

            //Assert
            Assert.AreEqual(1, int.Parse(node));

        }
    }
}
