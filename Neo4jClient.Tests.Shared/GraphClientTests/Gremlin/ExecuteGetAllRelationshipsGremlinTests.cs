using System;
using System.Net;
using Xunit;
using System.Linq;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.GraphClientTests.Gremlin
{
    
    public class ExecuteGetAllRelationshipsGremlinTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            Assert.Throws<InvalidOperationException>(() => client.ExecuteGetAllRelationshipsGremlin("", null));
        }

        [Fact]
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
                    Assert.Equal(1, relationships.Count());
                    Assert.Equal(456, relationships.ElementAt(0).Reference.Id);
                    Assert.Equal(123, relationships.ElementAt(0).StartNodeReference.Id);
                    Assert.Equal(789, relationships.ElementAt(0).EndNodeReference.Id);
                    Assert.Equal("KNOWS", relationships.ElementAt(0).TypeKey);
                }
    }

        [Fact]
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
                Assert.Equal(1, relationships.Count());
                Assert.Equal(456, relationships.ElementAt(0).Reference.Id);
                Assert.Equal(123, relationships.ElementAt(0).StartNodeReference.Id);
                Assert.Equal(789, relationships.ElementAt(0).EndNodeReference.Id);
                Assert.Equal("KNOWS", relationships.ElementAt(0).TypeKey);
                Assert.Equal("Foo", relationships.ElementAt(0).Data.Foo);
                Assert.Equal("Bar", relationships.ElementAt(0).Data.Bar);
            }
        }

        class TestPayload
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        [Fact]
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
                Assert.Equal(0, nodes.Count());
            }
        }

        [Fact]
        public void ShouldReturnListOfRelationshipInstancesWithLongRelationshipId()
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
                          'self' : 'http://127.0.0.1:5118/db/data/relationship/21484836470',
                          'property' : 'http://127.0.0.1:5118/db/data/relationship/21484836470/properties/{key}',
                          'properties' : 'http://127.0.0.1:5118/db/data/relationship/21484836470/properties',
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
                Assert.Equal(1, relationships.Count());
                Assert.Equal(21484836470, relationships.ElementAt(0).Reference.Id);
                Assert.Equal(123, relationships.ElementAt(0).StartNodeReference.Id);
                Assert.Equal(789, relationships.ElementAt(0).EndNodeReference.Id);
                Assert.Equal("KNOWS", relationships.ElementAt(0).TypeKey);
            }
        }

        [Fact]
        public void ShouldReturnListOfRelationshipInstancesWithLongStartNodeId()
        {
            //Arrange
            const string gremlinQueryExpected = "foo bar query";
            var query = new GremlinApiQuery(gremlinQueryExpected, null);

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/ext/GremlinPlugin/graphdb/execute_script", query),
                        MockResponse.Json(HttpStatusCode.OK, @"[ {
                          'start' : 'http://127.0.0.1:5118/db/data/node/21484836470',
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
                Assert.Equal(1, relationships.Count());
                Assert.Equal(456, relationships.ElementAt(0).Reference.Id);
                Assert.Equal(21484836470, relationships.ElementAt(0).StartNodeReference.Id);
                Assert.Equal(789, relationships.ElementAt(0).EndNodeReference.Id);
                Assert.Equal("KNOWS", relationships.ElementAt(0).TypeKey);
            }
        }

        [Fact]
        public void ShouldReturnListOfRelationshipInstancesWithLongEndNodeId()
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
                          'end' : 'http://127.0.0.1:5118/db/data/node/21484836470'
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
                Assert.Equal(1, relationships.Count());
                Assert.Equal(456, relationships.ElementAt(0).Reference.Id);
                Assert.Equal(123, relationships.ElementAt(0).StartNodeReference.Id);
                Assert.Equal(21484836470, relationships.ElementAt(0).EndNodeReference.Id);
                Assert.Equal("KNOWS", relationships.ElementAt(0).TypeKey);
            }
        }

        [Fact]
        public void ShouldFailGracefullyWhenGremlinIsNotAvailable()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot20()
                }
            })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                var ex = Assert.Throws<Exception>(
                    () => graphClient.ExecuteGetAllRelationshipsGremlin("foo bar query", null));
                Assert.Equal(GraphClient.GremlinPluginUnavailable, ex.Message);
            }
        }
    }
}