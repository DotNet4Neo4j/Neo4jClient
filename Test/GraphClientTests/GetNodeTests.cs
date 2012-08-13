using System;
using System.Net;
using NUnit.Framework;
using Neo4jClient.Gremlin;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class GetNodeTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.Get<object>(123);
        }

        [Test]
        public void ShouldReturnNodeData()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/node/456"),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo',
                                    'Bar': 'bar',
                                    'Baz': 'baz'
                          },
                          'create_relationship': 'http://foo/db/data/node/456/relationships',
                          'all_relationships': 'http://foo/db/data/node/456/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/456/properties',
                          'property': 'http://foo/db/data/node/456/property/{key}',
                          'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
                        }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var node = graphClient.Get<TestNode>(456);

                Assert.AreEqual(456, node.Reference.Id);
                Assert.AreEqual("foo", node.Data.Foo);
                Assert.AreEqual("bar", node.Data.Bar);
                Assert.AreEqual("baz", node.Data.Baz);
            }
        }

        [Test]
        public void ShouldReturnNodeDataAndDeserializeToEnumType()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/node/456"),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo',
                                    'Status': 'Value1'
                          },
                          'create_relationship': 'http://foo/db/data/node/456/relationships',
                          'all_relationships': 'http://foo/db/data/node/456/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/456/properties',
                          'property': 'http://foo/db/data/node/456/property/{key}',
                          'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
                        }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var node = graphClient.Get<TestNodeWithEnum>(456);

                Assert.AreEqual(456, node.Reference.Id);
                Assert.AreEqual("foo", node.Data.Foo);
                Assert.AreEqual(TestEnum.Value1, node.Data.Status);
            }
        }

        [Test]
        public void ShouldReturnNodeWithReferenceBackToClient()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/node/456"),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo',
                                    'Bar': 'bar',
                                    'Baz': 'baz'
                          },
                          'create_relationship': 'http://foo/db/data/node/456/relationships',
                          'all_relationships': 'http://foo/db/data/node/456/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/456/properties',
                          'property': 'http://foo/db/data/node/456/property/{key}',
                          'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
                        }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var node = graphClient.Get<TestNode>(456);

                Assert.AreEqual(graphClient, ((IGremlinQuery) node.Reference).Client);
            }
        }

        [Test]
        public void ShouldReturnNullWhenNodeDoesntExist()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/node/456"),
                    MockResponse.Http(404)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var node = graphClient.Get<TestNode>(456);

                Assert.IsNull(node);
            }
        }

        [Test]
        public void ShouldReturnNodeDataAndDeserialzedJsonDatesForDateTimeOffsetNullableType()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/node/456"),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'DateOffSet': '/Date(1309421746929+0000)/' },
                          'create_relationship': 'http://foo/db/data/node/456/relationships',
                          'all_relationships': 'http://foo/db/data/node/456/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/456/properties',
                          'property': 'http://foo/db/data/node/456/property/{key}',
                          'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
                        }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var node = graphClient.Get<TestNode>(456);

                Assert.IsNotNull(node.Data.DateOffSet);
                Assert.AreEqual("2011-06-30 08:15:46Z", node.Data.DateOffSet.Value.ToString("u"));
            }
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
            public DateTimeOffset? DateOffSet { get; set; }
        }

        public class TestNodeWithEnum
        {
            public string Foo { get; set; }
            public TestEnum Status { get; set; }
        }

        public enum TestEnum
        {
            Value1,
            Value2
        }
    }
}
