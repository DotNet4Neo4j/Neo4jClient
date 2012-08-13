using System.Linq;
using System.Net;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class GetIndexesTests
    {
        [Test]
        public void ShouldReturnNodeIndexes()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/index/node"),
                    MockResponse.Json(HttpStatusCode.OK, 
                        @"{
                            'agency24871-clients' : {
                            'to_lower_case' : 'true',
                            'template' : 'http://localhost:5102/db/data/index/node/agency24871-clients/{key}/{value}',
                            '_blueprints:type' : 'MANUAL',
                            'provider' : 'lucene',
                            'type' : 'fulltext'
                            },
                            'agency36681-clients' : {
                            'to_lower_case' : 'false',
                            'template' : 'http://localhost:5102/db/data/index/node/agency36681-clients/{key}/{value}',
                            '_blueprints:type' : 'MANUAL',
                            'provider' : 'lucene',
                            'type' : 'exact'
                            }
                        }")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var indexes = graphClient.GetIndexes(IndexFor.Node);
                Assert.AreEqual(2, indexes.Count());

                var index = indexes.ElementAt(0);
                Assert.AreEqual("agency24871-clients", index.Key);
                Assert.AreEqual(true, index.Value.ToLowerCase);
                Assert.AreEqual("http://localhost:5102/db/data/index/node/agency24871-clients/{key}/{value}", index.Value.Template);
                Assert.AreEqual("lucene", index.Value.Provider);
                Assert.AreEqual("fulltext", index.Value.Type);

                index = indexes.ElementAt(1);
                Assert.AreEqual("agency36681-clients", index.Key);
                Assert.AreEqual(false, index.Value.ToLowerCase);
                Assert.AreEqual("http://localhost:5102/db/data/index/node/agency36681-clients/{key}/{value}", index.Value.Template);
                Assert.AreEqual("lucene", index.Value.Provider);
                Assert.AreEqual("exact", index.Value.Type);
            }
        }

        [Test]
        public void ShouldReturnEmptyDictionaryOfIndexesForHttpResponse204()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/index/node"),
                    MockResponse.Http(204)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var indexes = graphClient.GetIndexes(IndexFor.Node);
                Assert.IsFalse(indexes.Any());
            }
        }
    }
}
