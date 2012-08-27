using System;
using System.Net;
using NUnit.Framework;
using System.Linq;
using Neo4jClient.ApiModels.Gremlin;

namespace Neo4jClient.Test.GraphClientTests.Gremlin
{
    [TestFixture]
    public class ExecuteGetAllRelationshipsGremlinTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.ExecuteGetAllRelationshipsGremlin("", null);
        }

        [Test]
        public void ShouldReturnListOfRelationshipInstances()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";
            var query = new GremlinApiQuery(gremlinQueryExpected, null);

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/ext/GremlinPlugin/graphdb/execute_script", query),
                        MockResponse.Json(HttpStatusCode.OK, @"[ {
                          'start' : 'http://127.0.0.1:5118/db/data/node/123',
                          'data' : {
                          },
                          'self' : 'http://127.0.0.1:5118/db/data/relationship/456',
                          'property' : 'http://127.0.0.1:5118/db/data/relationship/456/properties/{key}',
                          'properties' : 'http://127.0.0.1:5118/db/data/relationship/456/properties',
                          'type' : 'KNOWS',
                          'extensions' : {
                          },
                          'end' : 'http://127.0.0.1:5118/db/data/node/789'
                        } ]")
                    }
                }) 
                {
                    var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                    //Act
                    var relationships = graphClient
                        .ExecuteGetAllRelationshipsGremlin(gremlinQueryExpected, null)
                        .ToList();

                    //Assert
                    Assert.AreEqual(1, relationships.Count());
                    Assert.AreEqual(456, relationships.ElementAt(0).Reference.Id);
                    Assert.AreEqual(123, relationships.ElementAt(0).StartNodeReference.Id);
                    Assert.AreEqual(789, relationships.ElementAt(0).EndNodeReference.Id);
                    Assert.AreEqual("KNOWS", relationships.ElementAt(0).TypeKey);
                }
    }

        [Test]
        public void ShouldReturnListOfRelationshipInstancesWithPayloads()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";
            var query = new GremlinApiQuery(gremlinQueryExpected, null);

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/ext/GremlinPlugin/graphdb/execute_script", query),
                        MockResponse.Json(HttpStatusCode.OK, @"[ {
                          'start' : 'http://127.0.0.1:5118/db/data/node/123',
                          'data' : {
                            'Foo': 'Foo',
                            'Bar': 'Bar'
                          },
                          'self' : 'http://127.0.0.1:5118/db/data/relationship/456',
                          'property' : 'http://127.0.0.1:5118/db/data/relationship/456/properties/{key}',
                          'properties' : 'http://127.0.0.1:5118/db/data/relationship/456/properties',
                          'type' : 'KNOWS',
                          'extensions' : {
                          },
                          'end' : 'http://127.0.0.1:5118/db/data/node/789'
                        } ]")
                    }
                })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                //Act
                var relationships = graphClient
                    .ExecuteGetAllRelationshipsGremlin<TestPayload>(gremlinQueryExpected, null)
                    .ToList();

                //Assert
                Assert.AreEqual(1, relationships.Count());
                Assert.AreEqual(456, relationships.ElementAt(0).Reference.Id);
                Assert.AreEqual(123, relationships.ElementAt(0).StartNodeReference.Id);
                Assert.AreEqual(789, relationships.ElementAt(0).EndNodeReference.Id);
                Assert.AreEqual("KNOWS", relationships.ElementAt(0).TypeKey);
                Assert.AreEqual("Foo", relationships.ElementAt(0).Data.Foo);
                Assert.AreEqual("Bar", relationships.ElementAt(0).Data.Bar);
            }
        }

        class TestPayload
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        [Test]
        public void ShouldReturnEmptyEnumerableForNullResult()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";
            var query = new GremlinApiQuery(gremlinQueryExpected, null);


            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/ext/GremlinPlugin/graphdb/execute_script", query),
                        MockResponse.Json(HttpStatusCode.OK, @"[]")
                    }
                })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                //Act
                var nodes = graphClient
                    .ExecuteGetAllRelationshipsGremlin(gremlinQueryExpected, null)
                    .ToList();

                //Assert
                Assert.AreEqual(0, nodes.Count());
            }
        }
    }
}