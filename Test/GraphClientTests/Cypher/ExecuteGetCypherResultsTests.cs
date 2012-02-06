using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class ExecuteGetCypherResultsTests
    {
        public class ResultDto
        {
            public string RelationshipType { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
        }

        [Test]
        public void ShouldDeserializeSimpleTableStructure()
        {
            // Arrange
            const string queryText = @"
                START x = node({p0})
                MATCH x-[r]->n
                RETURN type(r) AS RelationshipType, n.Name? AS Name, n.UniqueId? AS UniqueId
                LIMIT 3";
            var query = new CypherQuery(
                queryText,
                new Dictionary<string, object>
                {
                    {"p0", 123}
                });

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest
                    {
                        Resource = "/",
                        Method = Method.GET
                    },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =
                            @"{
                                'cypher' : 'http://foo/db/data/cypher',
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
                    new RestRequest
                    {
                        Resource = "/cypher",
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
                    }.AddBody(new CypherApiQuery(query)),
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content =
                            @"{
                                'data' : [ [ 'HOSTS', 'foo', 44321 ], [ 'LIKES', 'bar', 44311 ], [ 'HOSTS', 'baz', 42586 ] ],
                                'columns' : [ 'RelationshipType', 'Name', 'UniqueId' ]
                            }".Replace('\'', '"')
                    }
                }
            });
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            //Act
            var results = graphClient.ExecuteGetCypherResults<ResultDto>(query);

            //Assert
            Assert.IsInstanceOf<IEnumerable<ResultDto>>(results);

            var resultsArray = results.ToArray();
            Assert.AreEqual(3, resultsArray.Count());

            var firstResult = resultsArray[0];
            Assert.AreEqual("HOSTS", firstResult.RelationshipType);
            Assert.AreEqual("foo", firstResult.Name);
            Assert.AreEqual(44321, firstResult.UniqueId);

            var secondResult = resultsArray[1];
            Assert.AreEqual("LIKES", secondResult.RelationshipType);
            Assert.AreEqual("bar", secondResult.Name);
            Assert.AreEqual(44311, secondResult.UniqueId);

            var thirdResult = resultsArray[2];
            Assert.AreEqual("HOSTS", thirdResult.RelationshipType);
            Assert.AreEqual("baz", thirdResult.Name);
            Assert.AreEqual(42586, thirdResult.UniqueId);
        }
    }
}