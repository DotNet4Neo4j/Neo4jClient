using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class UpdateRelationshipTests
    {
        [Test]
        public void ShouldUpdatePayload()
        {
            var testHarness = new RestTestHarness
            {
                {
                    new NeoHttpRequest { Resource = "", Method = Method.GET },
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
                          }
                        }".Replace('\'', '"')
                    }
                },
                 {
                    new NeoHttpRequest { Resource = "/relationship/456/properties", Method = Method.GET },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        TestContent = @"{ 'Foo': 'foo', 'Bar': 'bar', 'Baz': 'baz' }".Replace('\'', '"')
                    }
                },
                {
                    new NeoHttpRequest {
                        Resource = "/relationship/456/properties",
                        Method = Method.PUT,
                        RequestFormat = DataFormat.Json,
                        Body = new TestPayload { Foo = "fooUpdated", Bar = "bar", Baz = "bazUpdated" }
                    },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent
                    }
                }
            };

            var graphClient = testHarness.CreateAndConnectGraphClient();

            graphClient.Update(
                new RelationshipReference<TestPayload>(456),
                payloadFromDb =>
                {
                    payloadFromDb.Foo = "fooUpdated";
                    payloadFromDb.Baz = "bazUpdated";
                }
            );

            testHarness.AssertAllRequestsWereReceived();
        }

        [Test]
        public void ShouldInitializePayloadDuringUpdate()
        {
            var testHarness = new RestTestHarness
            {
                {
                    new NeoHttpRequest { Resource = "", Method = Method.GET },
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
                          }
                        }".Replace('\'', '"')
                    }
                },
                 {
                    new NeoHttpRequest { Resource = "/relationship/456/properties", Method = Method.GET },
                    new NeoHttpResponse { StatusCode = HttpStatusCode.NoContent }
                },
                {
                    new NeoHttpRequest {
                        Resource = "/relationship/456/properties",
                        Method = Method.PUT,
                        RequestFormat = DataFormat.Json,
                        Body = new TestPayload { Foo = "fooUpdated", Bar = "", Baz = "bazUpdated" }
                    },
                    new NeoHttpResponse {
                        StatusCode = HttpStatusCode.NoContent
                    }
                }
            };

            var graphClient = testHarness.CreateAndConnectGraphClient();

            graphClient.Update(
                new RelationshipReference<TestPayload>(456),
                payloadFromDb =>
                {
                    payloadFromDb.Foo = "fooUpdated";
                    payloadFromDb.Baz = "bazUpdated";
                }
            );

            testHarness.AssertAllRequestsWereReceived();
        }

        public class TestPayload
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }
    }
}