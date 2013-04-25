using NUnit.Framework;
using System.Linq;
using System.Net;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class LookupIndexTests
    {
        [Test]
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

                Assert.AreEqual(1, results.Count());
                var result = results[0];
                Assert.AreEqual(456, result.Reference.Id);
                Assert.AreEqual(1000, result.Data.Id);
            }
        }

        public class UserTestNode
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
    }
}
