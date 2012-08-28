using System.Linq;
using System.Net;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class QueryNodeIndexTests
    {
        [Test]
        public void ShouldReturnDataWhenQueryingAnIndex()
        {
            //Arrange
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/index/node/indexName?query=name%3dfoo"),
                    MockResponse.Json(HttpStatusCode.OK, @"[ {
                        'outgoing_relationships' : 'http://foo/db/data/node/169/relationships/out',
                        'data' : {
                        'name' : 'foo'
                        },
                        'traverse' : 'http://foo/db/data/node/169/traverse/{returnType}',
                        'all_typed_relationships' : 'http://foo/db/data/node/169/relationships/all/{-list|&|types}',
                        'property' : 'http://foo/db/data/node/169/properties/{key}',
                        'self' : 'http://foo/db/data/node/169',
                        'outgoing_typed_relationships' : 'http://foo/db/data/node/169/relationships/out/{-list|&|types}',
                        'properties' : 'http://foo/db/data/node/169/properties',
                        'incoming_relationships' : 'http://foo/db/data/node/169/relationships/in',
                        'extensions' : {
                        },
                        'create_relationship' : 'http://foo/db/data/node/169/relationships',
                        'paged_traverse' : 'http://foo/db/data/node/169/paged/traverse/{returnType}{?pageSize,leaseTime}',
                        'all_relationships' : 'http://foo/db/data/node/169/relationships/all',
                        'incoming_typed_relationships' : 'http://foo/db/data/node/169/relationships/in/{-list|&|types}'
                    } ]")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                //var results = graphClient
                //    .QueryIndex<TestNode>("indexName", IndexFor.Node, "name:foo")
                //    .ToArray();

                // Assert
                //Assert.AreEqual(1, results.Count());
                //var result = results.ElementAt(0);
                //Assert.AreEqual(169, result.Reference.Id);
                //Assert.AreEqual("foo", result.Data.Name);

                Assert.Ignore("Need to fix mocking params in a GET");
            }
        }

        public class TestNode
        {
            public string Name { get; set; }
        }
    }
}
