using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using System.Linq;

namespace Neo4jClient.Test.GraphClientTests.Gremlin
{
    [TestFixture]
    public class ExecuteGetAllNodesGremlinTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.ExecuteGetAllNodesGremlin<object>("", null);
        }

        public class Foo
        {
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        [Test]
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
                Assert.AreEqual(2, nodes.Count());
                Assert.AreEqual(5, nodes.ElementAt(0).Reference.Id);
                Assert.AreEqual("bar", nodes.ElementAt(0).Data.Bar);
                Assert.AreEqual("baz", nodes.ElementAt(0).Data.Baz);
                Assert.AreEqual(6, nodes.ElementAt(1).Reference.Id);
                Assert.AreEqual("123", nodes.ElementAt(1).Data.Bar);
                Assert.AreEqual("456", nodes.ElementAt(1).Data.Baz);
            }
        }

        [Test]
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
                Assert.AreEqual(0, nodes.Count());
            }
        }
    }
}