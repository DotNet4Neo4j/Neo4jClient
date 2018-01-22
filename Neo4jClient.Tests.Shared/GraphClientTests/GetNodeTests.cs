using System;
using System.Net;
using Xunit;
using Neo4jClient.Gremlin;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class GetNodeTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            Assert.Throws<InvalidOperationException>(() => client.Get<object>((NodeReference)123));
        }

        [Fact]
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
                var node = graphClient.Get<TestNode>((NodeReference)456);

                Assert.Equal(456, node.Reference.Id);
                Assert.Equal("foo", node.Data.Foo);
                Assert.Equal("bar", node.Data.Bar);
                Assert.Equal("baz", node.Data.Baz);
            }
        }

        [Fact]
        public void ShouldReturnNodeDataForLongId()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/node/21484836470"),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"{ 'self': 'http://foo/db/data/node/21484836470',
                          'data': { 'Foo': 'foo',
                                    'Bar': 'bar',
                                    'Baz': 'baz'
                          },
                          'create_relationship': 'http://foo/db/data/node/21484836470/relationships',
                          'all_relationships': 'http://foo/db/data/node/21484836470/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/21484836470/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/21484836470/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/21484836470/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/21484836470/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/21484836470/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/21484836470/properties',
                          'property': 'http://foo/db/data/node/21484836470/property/{key}',
                          'traverse': 'http://foo/db/data/node/21484836470/traverse/{returnType}'
                        }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var node = graphClient.Get<TestNode>((NodeReference)21484836470);

                Assert.Equal(21484836470, node.Reference.Id);
                Assert.Equal("foo", node.Data.Foo);
                Assert.Equal("bar", node.Data.Bar);
                Assert.Equal("baz", node.Data.Baz);
            }
        }

        [Fact]
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
                var node = graphClient.Get<TestNodeWithEnum>((NodeReference)456);

                Assert.Equal(456, node.Reference.Id);
                Assert.Equal("foo", node.Data.Foo);
                Assert.Equal(TestEnum.Value1, node.Data.Status);
            }
        }

        [Fact]
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
                var node = graphClient.Get<TestNode>((NodeReference)456);

                Assert.Equal(graphClient, ((IGremlinQuery) node.Reference).Client);
            }
        }

        [Fact]
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
                var node = graphClient.Get<TestNode>((NodeReference)456);

                Assert.Null(node);
            }
        }

        [Fact]
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
                var node = graphClient.Get<TestNode>((NodeReference)456);

                Assert.NotNull(node.Data.DateOffSet);
                Assert.Equal("2011-06-30 08:15:46Z", node.Data.DateOffSet.Value.ToString("u"));
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
