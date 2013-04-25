using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class ReIndexTests
    {
        #region ReIndex Nodes
        [Test]
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
                var nodeReference = new NodeReference<TestNode>(123);
                graphClient.ReIndex(nodeReference, indexEntries);

                // Assert
                Assert.Pass("Success.");
            }
        }

        [Test]
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
                var nodeReference = new NodeReference<TestNode>(123);
                graphClient.ReIndex(nodeReference, indexEntries);

                // Assert
                Assert.Pass("Success.");
            }
        }

        [Test]
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
                var nodeReference = new NodeReference<TestNode>(123);
                graphClient.ReIndex(nodeReference, indexEntries);

                // Assert
                Assert.Pass("Success.");
            }
        }

        [Test]
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
                var nodeReference = new NodeReference<TestNode>(123);
                graphClient.ReIndex(nodeReference, indexEntries);

                // Assert
                Assert.Pass("Success.");
            }
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        public void ShouldThrowNotSupportExceptionForPre15M02Database()
        {
            //Arrange

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.Get(""),
                        MockResponse.NeoRootPre15M02()
                    }
                })
            {

                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var nodeReference = new NodeReference<TestNode>(123);
                graphClient.ReIndex(nodeReference, new IndexEntry[0]);
            }
        }
        #endregion

        #region ReIndex Relationships
        [Test]
        public void ShouldReindexRelationshipWithIndexEntryContainingSpace()
        {
            //Arrange
            var indexEntries = new List<IndexEntry>
                {
                    new IndexEntry
                        {
                            Name = "my_relationships",
                            KeyValues = new Dictionary<string, object>
                                {
                                    {"BarKey", "the_value with space"}
                                },
                        }
                };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/index/relationship/my_relationships",
                                                     new
                                                         {
                                                             key = "BarKey",
                                                             value = "the_value with space",
                                                             uri = "http://foo/db/data/relationship/1234"
                                                         }),
                        MockResponse.Json(HttpStatusCode.Created,
                                          @"Location: http://foo/db/data/index/relationship/my_relationships/BarKey/the_value%20with%20space/1234")
                    },
                    {
                        MockRequest.Delete("/index/relationship/my_relationships/1234"),
                        MockResponse.Http((int) HttpStatusCode.NoContent)
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var relReference = new RelationshipReference(1234);
                graphClient.ReIndex(relReference, indexEntries);

                // Assert
                Assert.Pass("Success.");
            }
        }

        [Test]
        public void ShouldReindexRelationshipWithDateTimeOffsetIndexEntry()
        {
            //Arrange
            var indexEntries = new List<IndexEntry>
                {
                    new IndexEntry
                        {
                            Name = "my_relationships",
                            KeyValues = new Dictionary<string, object>
                                {
                                    {"BarKey", new DateTimeOffset(1000, new TimeSpan(0))}
                                },
                        }
                };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/index/relationship/my_relationships",
                                                     new
                                                         {
                                                             key = "BarKey",
                                                             value = "1000",
                                                             uri = "http://foo/db/data/relationship/1234"
                                                         }),
                        MockResponse.Json(HttpStatusCode.Created,
                                          @"Location: http://foo/db/data/index/relationship/my_relationships/BarKey/someDateValue/1234")
                    },
                    {
                        MockRequest.Delete("/index/relationship/my_relationships/1234"),
                        MockResponse.Http((int) HttpStatusCode.NoContent)
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var relReference = new RelationshipReference(1234);
                graphClient.ReIndex(relReference, indexEntries);

                // Assert
                Assert.Pass("Success.");
            }
        }

        [Test]
        public void ShouldAcceptQuestionMarkInRelationshipIndexValue()
        {
            //Arrange
            var indexKeyValues = new Dictionary<string, object>
                {
                    {"BarKey", "foo?bar"}
                };
            var indexEntries = new List<IndexEntry>
                {
                    new IndexEntry
                        {
                            Name = "my_relationships",
                            KeyValues = indexKeyValues,
                        }
                };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/index/relationship/my_relationships",
                                                     new
                                                         {
                                                             key = "BarKey",
                                                             value = "foo?bar",
                                                             uri = "http://foo/db/data/relationship/1234"
                                                         }),
                        MockResponse.Json(HttpStatusCode.Created,
                                          @"Location: http://foo/db/data/index/relationship/my_relationships/BarKey/%3f/1234")
                    },
                    {
                        MockRequest.Delete("/index/relationship/my_relationships/1234"),
                        MockResponse.Http((int) HttpStatusCode.NoContent)
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var relReference = new RelationshipReference(1234);
                graphClient.ReIndex(relReference, indexEntries);

                // Assert
                Assert.Pass("Success.");
            }
        }

        [Test]
        public void ShouldPreserveSlashInRelationshipIndexValue()
        {
            //Arrange
            var indexKeyValues = new Dictionary<string, object>
                {
                    {"BarKey", "abc/def"}
                };
            var indexEntries = new List<IndexEntry>
                {
                    new IndexEntry
                        {
                            Name = "my_relationships",
                            KeyValues = indexKeyValues,
                        }
                };

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/index/relationship/my_relationships",
                                                     new
                                                         {
                                                             key = "BarKey",
                                                             value = "abc/def",
                                                             uri = "http://foo/db/data/relationship/1234"
                                                         }),
                        MockResponse.Json(HttpStatusCode.Created,
                                          @"Location: http://foo/db/data/index/relationship/my_relationships/BarKey/abc-def/1234")
                    },
                    {
                        MockRequest.Delete("/index/relationship/my_relationships/123"),
                        MockResponse.Http((int) HttpStatusCode.NoContent)
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                var relReference = new RelationshipReference(123);
                graphClient.ReIndex(relReference, indexEntries);

                // Assert
                Assert.Pass("Success.");
            }
        }
        #endregion

        public class TestNode
        {
            public string FooKey { get; set; }
        }

        public class TestNode2
        {
        }

        public class TestRelationship : Relationship,
            IRelationshipAllowingSourceNode<TestNode>,
            IRelationshipAllowingTargetNode<TestNode2>
        {
            public TestRelationship(NodeReference targetNode)
                : base(targetNode)
            {
            }

            public override string RelationshipTypeKey
            {
                get { return "TEST_RELATIONSHIP"; }
            }
        }

    }
}