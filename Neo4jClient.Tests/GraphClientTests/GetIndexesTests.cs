﻿/*
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class GetIndexesTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task ShouldReturnNodeIndexes()
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
                var graphClient = await testHarness.CreateAndConnectGraphClient();
                var indexes = await graphClient.GetIndexesAsync(IndexFor.Node);
                Assert.Equal(2, indexes.Count());

                var index = indexes.ElementAt(0);
                Assert.Equal("agency24871-clients", index.Key);
                Assert.Equal(true, index.Value.ToLowerCase);
                Assert.Equal("http://localhost:5102/db/data/index/node/agency24871-clients/{key}/{value}", index.Value.Template);
                Assert.Equal("lucene", index.Value.Provider);
                Assert.Equal("fulltext", index.Value.Type);

                index = indexes.ElementAt(1);
                Assert.Equal("agency36681-clients", index.Key);
                Assert.Equal(false, index.Value.ToLowerCase);
                Assert.Equal("http://localhost:5102/db/data/index/node/agency36681-clients/{key}/{value}", index.Value.Template);
                Assert.Equal("lucene", index.Value.Provider);
                Assert.Equal("exact", index.Value.Type);
            }
        }

        [Fact]
        public async Task ShouldReturnEmptyDictionaryOfIndexesForHttpResponse204()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/index/node"),
                    MockResponse.Http(204)
                }
            })
            {
                var graphClient = await testHarness.CreateAndConnectGraphClient();
                var indexes = await graphClient.GetIndexesAsync(IndexFor.Node);
                Assert.False(indexes.Any());
            }
        }
    }
}
*/
