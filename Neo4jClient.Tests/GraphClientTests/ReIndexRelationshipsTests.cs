// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Threading.Tasks;
// using Xunit;
//
// namespace Neo4jClient.Tests.GraphClientTests
// {
//     
//     public class ReIndexRelationshipsTests : IClassFixture<CultureInfoSetupFixture>
//     {
//         [Fact]
//         public async Task ShouldReindexRelationshipWithIndexEntryContainingSpace()
//         {
//             //Arrange
//             var indexEntries = new List<IndexEntry>
//                 {
//                     new IndexEntry
//                         {
//                             Name = "my_relationships",
//                             KeyValues = new Dictionary<string, object>
//                                 {
//                                     {"BarKey", "the_value with space"}
//                                 },
//                         }
//                 };
//
//             using (var testHarness = new RestTestHarness
//                 {
//                     {
//                         MockRequest.PostObjectAsJson("/index/relationship/my_relationships",
//                                                      new
//                                                          {
//                                                              key = "BarKey",
//                                                              value = "the_value with space",
//                                                              uri = "http://foo/db/data/relationship/1234"
//                                                          }),
//                         MockResponse.Json(HttpStatusCode.Created,
//                                           @"Location: http://foo/db/data/index/relationship/my_relationships/BarKey/the_value%20with%20space/1234")
//                     },
//                     {
//                         MockRequest.Delete("/index/relationship/my_relationships/1234"),
//                         MockResponse.Http((int) HttpStatusCode.NoContent)
//                     }
//                 })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 var relReference = new RelationshipReference(1234);
//                 await graphClient.ReIndexAsync(relReference, indexEntries);
//
//                 // Assert
//                 
//             }
//         }
//
//         [Fact]
//         public async Task ShouldReindexRelationshipWithDateTimeOffsetIndexEntry()
//         {
//             //Arrange
//             var indexEntries = new List<IndexEntry>
//                 {
//                     new IndexEntry
//                         {
//                             Name = "my_relationships",
//                             KeyValues = new Dictionary<string, object>
//                                 {
//                                     {"BarKey", new DateTimeOffset(1000, new TimeSpan(0))}
//                                 },
//                         }
//                 };
//
//             using (var testHarness = new RestTestHarness
//                 {
//                     {
//                         MockRequest.PostObjectAsJson("/index/relationship/my_relationships",
//                                                      new
//                                                          {
//                                                              key = "BarKey",
//                                                              value = "1000",
//                                                              uri = "http://foo/db/data/relationship/1234"
//                                                          }),
//                         MockResponse.Json(HttpStatusCode.Created,
//                                           @"Location: http://foo/db/data/index/relationship/my_relationships/BarKey/someDateValue/1234")
//                     },
//                     {
//                         MockRequest.Delete("/index/relationship/my_relationships/1234"),
//                         MockResponse.Http((int) HttpStatusCode.NoContent)
//                     }
//                 })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 var relReference = new RelationshipReference(1234);
//                 await graphClient.ReIndexAsync(relReference, indexEntries);
//
//                 // Assert
//                 
//             }
//         }
//
//         [Fact]
//         public async Task ShouldAcceptQuestionMarkInRelationshipIndexValue()
//         {
//             //Arrange
//             var indexKeyValues = new Dictionary<string, object>
//                 {
//                     {"BarKey", "foo?bar"}
//                 };
//             var indexEntries = new List<IndexEntry>
//                 {
//                     new IndexEntry
//                         {
//                             Name = "my_relationships",
//                             KeyValues = indexKeyValues,
//                         }
//                 };
//
//             using (var testHarness = new RestTestHarness
//                 {
//                     {
//                         MockRequest.PostObjectAsJson("/index/relationship/my_relationships",
//                                                      new
//                                                          {
//                                                              key = "BarKey",
//                                                              value = "foo?bar",
//                                                              uri = "http://foo/db/data/relationship/1234"
//                                                          }),
//                         MockResponse.Json(HttpStatusCode.Created,
//                                           @"Location: http://foo/db/data/index/relationship/my_relationships/BarKey/%3f/1234")
//                     },
//                     {
//                         MockRequest.Delete("/index/relationship/my_relationships/1234"),
//                         MockResponse.Http((int) HttpStatusCode.NoContent)
//                     }
//                 })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 var relReference = new RelationshipReference(1234);
//                 await graphClient.ReIndexAsync(relReference, indexEntries);
//
//                 // Assert
//                 
//             }
//         }
//
//         [Fact]
//         public async Task ShouldPreserveSlashInRelationshipIndexValue()
//         {
//             //Arrange
//             var indexKeyValues = new Dictionary<string, object>
//                 {
//                     {"BarKey", "abc/def"}
//                 };
//             var indexEntries = new List<IndexEntry>
//                 {
//                     new IndexEntry
//                         {
//                             Name = "my_relationships",
//                             KeyValues = indexKeyValues,
//                         }
//                 };
//
//             using (var testHarness = new RestTestHarness
//                 {
//                     {
//                         MockRequest.PostObjectAsJson("/index/relationship/my_relationships",
//                                                      new
//                                                          {
//                                                              key = "BarKey",
//                                                              value = "abc/def",
//                                                              uri = "http://foo/db/data/relationship/123"
//                                                          }),
//                         MockResponse.Json(HttpStatusCode.Created,
//                                           @"Location: http://foo/db/data/index/relationship/my_relationships/BarKey/abc-def/1234")
//                     },
//                     {
//                         MockRequest.Delete("/index/relationship/my_relationships/123"),
//                         MockResponse.Http((int) HttpStatusCode.NoContent)
//                     }
//                 })
//             {
//                 var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                 //Act
//                 var relReference = new RelationshipReference(123);
//                 await graphClient.ReIndexAsync(relReference, indexEntries);
//
//                 // Assert
//                 
//             }
//         }
//
//         public class TestRelationship : Relationship
//         {
//             public TestRelationship(NodeReference targetNode)
//                 : base(targetNode)
//             {
//             }
//
//             public override string RelationshipTypeKey
//             {
//                 get { return "TEST_RELATIONSHIP"; }
//             }
//         }
//
//     }
// }