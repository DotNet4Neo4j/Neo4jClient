/*
using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class DeleteNodeTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.DeleteAsync(123, DeleteMode.NodeOnly));
        }

        [Fact]
        public async Task ShouldDeleteNodeOnly()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Delete("/node/456"),
                    MockResponse.Http(204)
                }
            })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();
                await graphClient.DeleteAsync(456, DeleteMode.NodeOnly);
            }
        }

        [Fact]
        public async Task ShouldDeleteAllRelationshipsFirst()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/node/456/relationships/all"),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[
                          { 'self': 'http://foo/db/data/relationship/56',
                            'start': 'http://foo/db/data/node/123',
                            'end': 'http://foo/db/data/node/456',
                            'type': 'KNOWS',
                            'properties': 'http://foo/db/data/relationship/56/properties',
                            'property': 'http://foo/db/data/relationship/56/properties/{key}',
                            'data': { 'date': 1270559208258 }
                          },
                          { 'self': 'http://foo/db/data/relationship/78',
                            'start': 'http://foo/db/data/node/456',
                            'end': 'http://foo/db/data/node/789',
                            'type': 'KNOWS',
                            'properties': 'http://foo/db/data/relationship/78/properties',
                            'property': 'http://foo/db/data/relationship/78/properties/{key}',
                            'data': { 'date': 1270559208258 }
                          }
                        ]")
                },
                {
                    MockRequest.Delete("/relationship/56"),
                    MockResponse.Http(204)
                },
                {
                    MockRequest.Delete("/relationship/78"),
                    MockResponse.Http(204)
                },
                {
                    MockRequest.Delete("/node/456"),
                    MockResponse.Http(204)
                }
            })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();
                await graphClient.DeleteAsync(456, DeleteMode.NodeAndRelationships);
            }
        }

        [Fact]
        public async Task ShouldThrowExceptionWhenDeleteFails()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Delete("/node/456"),
                    MockResponse.Http(409)
                }
            })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();
                var ex = await Assert.ThrowsAsync<Exception>(async () => await graphClient.DeleteAsync(456, DeleteMode.NodeOnly));
                ex.Message.Should().Be("Unable to delete the node. The node may still have relationships. The response status was: 409 Conflict");

            }
        }
    }
}
*/
