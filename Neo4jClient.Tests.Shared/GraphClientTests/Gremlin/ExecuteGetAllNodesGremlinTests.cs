using System;
using System.Collections.Generic;
using System.Net;
using Xunit;
using System.Linq;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.GraphClientTests.Gremlin
{
    
    public class ExecuteGetAllNodesGremlinTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            Assert.Throws<InvalidOperationException>(() => client.ExecuteGetAllNodesGremlin<object>("", null));
        }

        public class Foo
        {
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        [Fact]
        public void ShouldReturnIEnumerableOfObjects()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson(
                        "/ext/GremlinPlugin/graphdb/execute_script",
                        @"{
                            'script': 'foo bar query',
                            'params': { 'foo': 123, 'bar': 'baz' }
                        }"
                    ),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[ {
                            'outgoing_relationships' : 'http://foo/db/data/node/5/relationships/out',
                            'data' : {
                                'Bar' : 'bar',
                                'Baz' : 'baz'
                            },
                            'traverse' : 'http://foo/db/data/node/5/traverse/{returnType}',
                            'all_typed_relationships' : 'http://foo/db/data/node/5/relationships/all/{-list|&|types}',
                            'property' : 'http://foo/db/data/node/5/properties/{key}',
                            'self' : 'http://foo/db/data/node/5',
                            'properties' : 'http://foo/db/data/node/5/properties',
                            'outgoing_typed_relationships' : 'http://foo/db/data/node/5/relationships/out/{-list|&|types}',
                            'incoming_relationships' : 'http://foo/db/data/node/5/relationships/in',
                            'extensions' : {
                            },
                            'create_relationship' : 'http://foo/db/data/node/5/relationships',
                            'all_relationships' : 'http://foo/db/data/node/5/relationships/all',
                            'incoming_typed_relationships' : 'http://foo/db/data/node/5/relationships/in/{-list|&|types}'
                        }, {
                            'outgoing_relationships' : 'http://foo/db/data/node/6/relationships/out',
                            'data' : {
                                'Bar' : '123',
                                'Baz' : '456'
                            },
                            'traverse' : 'http://foo/db/data/node/6/traverse/{returnType}',
                            'all_typed_relationships' : 'http://foo/db/data/node/6/relationships/all/{-list|&|types}',
                            'property' : 'http://foo/db/data/node/6/properties/{key}',
                            'self' : 'http://foo/db/data/node/6',
                            'properties' : 'http://foo/db/data/node/6/properties',
                            'outgoing_typed_relationships' : 'http://foo/db/data/node/6/relationships/out/{-list|&|types}',
                            'incoming_relationships' : 'http://foo/db/data/node/6/relationships/in',
                            'extensions' : {
                            },
                            'create_relationship' : 'http://foo/db/data/node/6/relationships',
                            'all_relationships' : 'http://foo/db/data/node/6/relationships/all',
                            'incoming_typed_relationships' : 'http://foo/db/data/node/6/relationships/in/{-list|&|types}'
                        } ]"
                    )
                }
            })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                //Act
                var parameters = new Dictionary<string, object>
                {
                    {"foo", 123},
                    {"bar", "baz"}
                };
                var nodes = graphClient
                    .ExecuteGetAllNodesGremlin<Foo>("foo bar query", parameters)
                    .ToList();

                //Assert
                Assert.Equal(2, nodes.Count());
                Assert.Equal(5, nodes.ElementAt(0).Reference.Id);
                Assert.Equal("bar", nodes.ElementAt(0).Data.Bar);
                Assert.Equal("baz", nodes.ElementAt(0).Data.Baz);
                Assert.Equal(6, nodes.ElementAt(1).Reference.Id);
                Assert.Equal("123", nodes.ElementAt(1).Data.Bar);
                Assert.Equal("456", nodes.ElementAt(1).Data.Baz);
            }
        }

        [Fact]
        public void ShouldReturnEmptyEnumerableForNullResult()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson(
                        "/ext/GremlinPlugin/graphdb/execute_script",
                        @"{ 'script': 'foo bar query', 'params': {} }"
                    ),
                    MockResponse.Json(HttpStatusCode.OK, @"[]")
                }
            })
            {
                var graphClient = (GraphClient)testHarness.CreateAndConnectGraphClient();

                //Act
                var nodes = graphClient
                    .ExecuteGetAllNodesGremlin<Foo>("foo bar query", null)
                    .ToList();

                //Assert
                Assert.Equal(0, nodes.Count());
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
                    () => graphClient.ExecuteGetAllNodesGremlin<Foo>("foo bar query", null));
                Assert.Equal(GraphClient.GremlinPluginUnavailable, ex.Message);
            }
        }
    }
}