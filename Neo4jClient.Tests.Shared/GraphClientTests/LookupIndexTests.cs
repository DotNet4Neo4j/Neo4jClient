using Xunit;
using System.Linq;
using System.Net;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class LookupIndexTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldReturnLookupIndexResult()
        {
            //Arrange
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/index/node/users/Id/1000"),
                    MockResponse.Json(HttpStatusCode.OK, 
                        @"[{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Id': '1000', 'Name': 'Foo' },
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
                        }]")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                var results = graphClient
                    .LookupIndex<UserTestNode>("users", IndexFor.Node, "Id", 1000)
                    .ToArray();

                Assert.Equal(1, results.Count());
                var result = results[0];
                Assert.Equal(456, result.Reference.Id);
                Assert.Equal(1000, result.Data.Id);
            }
        }

        public class UserTestNode
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
    }
}
