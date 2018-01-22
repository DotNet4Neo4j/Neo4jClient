using System;
using System.Collections.Generic;
using System.Net;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class ReIndexNodesTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldReindexNodeWithIndexEntryContainingSpace()
        {
            //Arrange
            var indexEntries = new List<IndexEntry>
                {
                    new IndexEntry
                        {
                            Name = "my_nodes",
                            KeyValues = new Dictionary<string, object>
                                {
                                    {"FooKey", "the_value with space"}
                                },
                        }
                };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/index/node/my_nodes",
                                                     new
                                                         {
                                                             key = "FooKey",
                                                             value = "the_value with space",
                                                             uri = "http://foo/db/data/node/123"
                                                         }),
                        MockResponse.Json(HttpStatusCode.Created,
                                          @"Location: http://foo/db/data/index/node/my_nodes/FooKey/the_value%20with%20space/123")
                    },
                    {
                        MockRequest.Delete("/index/node/my_nodes/123"),
                        MockResponse.Http((int) HttpStatusCode.NoContent)
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                graphClient.ReIndex((NodeReference)123, indexEntries);

                // Assert
                
            }
        }

        [Fact]
        public void ShouldReindexNodeWithDateTimeOffsetIndexEntry()
        {
            //Arrange
            var indexEntries = new List<IndexEntry>
                {
                    new IndexEntry
                        {
                            Name = "my_nodes",
                            KeyValues = new Dictionary<string, object>
                                {
                                    {"FooKey", new DateTimeOffset(1000, new TimeSpan(0))}
                                },
                        }
                };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/index/node/my_nodes",
                                                     new
                                                         {
                                                             key = "FooKey",
                                                             value = "1000",
                                                             uri = "http://foo/db/data/node/123"
                                                         }),
                        MockResponse.Json(HttpStatusCode.Created,
                                          @"Location: http://foo/db/data/index/node/my_nodes/FooKey/someDateValue/123")
                    },
                    {
                        MockRequest.Delete("/index/node/my_nodes/123"),
                        MockResponse.Http((int) HttpStatusCode.NoContent)
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                graphClient.ReIndex((NodeReference)123, indexEntries);

                // Assert
                
            }
        }

        [Fact]
        public void ShouldAcceptQuestionMarkInIndexValue()
        {
            //Arrange
            var indexKeyValues = new Dictionary<string, object>
                {
                    {"FooKey", "foo?bar"}
                };
            var indexEntries = new List<IndexEntry>
                {
                    new IndexEntry
                        {
                            Name = "my_nodes",
                            KeyValues = indexKeyValues,
                        }
                };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/index/node/my_nodes",
                                                     new
                                                         {
                                                             key = "FooKey",
                                                             value = "foo?bar",
                                                             uri = "http://foo/db/data/node/123"
                                                         }),
                        MockResponse.Json(HttpStatusCode.Created,
                                          @"Location: http://foo/db/data/index/node/my_nodes/FooKey/%3f/123")
                    },
                    {
                        MockRequest.Delete("/index/node/my_nodes/123"),
                        MockResponse.Http((int) HttpStatusCode.NoContent)
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                graphClient.ReIndex((NodeReference)123, indexEntries);

                // Assert
                
            }
        }

        [Fact]
        public void ShouldPreserveSlashInIndexValue()
        {
            //Arrange
            var indexKeyValues = new Dictionary<string, object>
                {
                    {"FooKey", "abc/def"}
                };
            var indexEntries = new List<IndexEntry>
                {
                    new IndexEntry
                        {
                            Name = "my_nodes",
                            KeyValues = indexKeyValues,
                        }
                };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/index/node/my_nodes",
                                                     new
                                                         {
                                                             key = "FooKey",
                                                             value = "abc/def",
                                                             uri = "http://foo/db/data/node/123"
                                                         }),
                        MockResponse.Json(HttpStatusCode.Created,
                                          @"Location: http://foo/db/data/index/node/my_nodes/FooKey/abc-def/123")
                    },
                    {
                        MockRequest.Delete("/index/node/my_nodes/123"),
                        MockResponse.Http((int) HttpStatusCode.NoContent)
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                graphClient.ReIndex((NodeReference)123, indexEntries);

                // Assert
                
            }
        }
    }
}
