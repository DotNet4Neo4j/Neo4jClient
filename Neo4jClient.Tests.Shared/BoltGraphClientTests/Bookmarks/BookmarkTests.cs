using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Neo4j.Driver.V1;
using Neo4jClient.Cypher;
using Neo4jClient.Test.BoltGraphClientTests.Cypher;
using Neo4jClient.Test.Fixtures;
using Neo4jClient.Transactions;
using Xunit;

namespace Neo4jClient.Test.BoltGraphClientTests
{
    public class BookmarkTests : IClassFixture<CultureInfoSetupFixture>
    {
        public class OperationCompletedEvent : IClassFixture<CultureInfoSetupFixture>
        {
            private class ObjectWithIds
            {
                public List<int> Ids { get; set; }
            }

            [Fact]
            public void ArgsContainBookmarksUsed()
            {
                // Arrange
                var bookmarks = new List<string> {"Bookmark1", "Bookmark2"};

                const string queryText = "RETURN [] AS data";

                var queryParams = new Dictionary<string, object>();

                var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional) {Bookmarks = bookmarks};

                using (var testHarness = new BoltTestHarness())
                {
                    var recordMock = new Mock<IRecord>();
                    recordMock.Setup(r => r["data"]).Returns(new List<INode>());
                    recordMock.Setup(r => r.Keys).Returns(new[] {"data"});

                    var testStatementResult = new TestStatementResult(new[] {"data"}, recordMock.Object);
                    testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                    var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                    graphClient.OperationCompleted += (s, e) =>
                    {
                        e.BookmarksUsed.Should().Contain(bookmarks[0]);
                        e.BookmarksUsed.Should().Contain(bookmarks[1]);
                    };

                    graphClient.ExecuteGetCypherResults<IEnumerable<ObjectWithIds>>(cypherQuery);
                }
            }


            [Fact]
            public void ArgsContainBookmarkUsed()
            {
                // Arrange
                const string bookmark = "Bookmark1";
                var bookmarks = new List<string> {bookmark};

                const string queryText = "RETURN [] AS data";

                var queryParams = new Dictionary<string, object>();

                var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional) {Bookmarks = bookmarks};

                using (var testHarness = new BoltTestHarness())
                {
                    var recordMock = new Mock<IRecord>();
                    recordMock.Setup(r => r["data"]).Returns(new List<INode>());
                    recordMock.Setup(r => r.Keys).Returns(new[] {"data"});

                    var testStatementResult = new TestStatementResult(new[] {"data"}, recordMock.Object);
                    testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                    var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                    graphClient.OperationCompleted += (s, e) => { e.BookmarksUsed.Should().Contain(bookmarks[0]); };

                    graphClient.ExecuteGetCypherResults<IEnumerable<ObjectWithIds>>(cypherQuery);
                }
            }

            [Fact]
            public void ArgsContainLastBookmark()
            {
                const string lastBookmark = "LastBookmark";

                const string queryText = "RETURN [] AS data";

                var queryParams = new Dictionary<string, object>();

                var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional);

                using (var testHarness = new BoltTestHarness())
                {
                    var recordMock = new Mock<IRecord>();
                    recordMock.Setup(r => r["data"]).Returns(new List<INode>());
                    recordMock.Setup(r => r.Keys).Returns(new[] {"data"});

                    testHarness.MockSession.Setup(s => s.LastBookmark).Returns(lastBookmark);

                    var testStatementResult = new TestStatementResult(new[] {"data"}, recordMock.Object);
                    testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                    var graphClient = testHarness.CreateAndConnectBoltGraphClient();
                    graphClient.OperationCompleted += (s, e) => { e.LastBookmark.Should().Be(lastBookmark); };

                    graphClient.ExecuteGetCypherResults<IEnumerable<ObjectWithIds>>(cypherQuery);
                }
            }

            [Fact]
            public void SessionIsCalledWithBookmark()
            {
                // Arrange
                const string bookmark = "Bookmark1";

                var cypherQuery = new CypherQuery("RETURN 1", new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional) {Bookmarks = new List<string> {bookmark}};

                using (var testHarness = new BoltTestHarness())
                {
                    try
                    {
                        testHarness.CreateAndConnectBoltGraphClient().ExecuteGetCypherResults<object>(cypherQuery).ToArray();
                    }
                    catch
                    {
                        /*Not interested in actually getting results*/
                    }

                    //Assert
                    testHarness.MockDriver.Verify(d => d.Session(It.IsAny<AccessMode>(), It.Is<IEnumerable<string>>(s => s.Contains(bookmark))), Times.Once);
                }
            }

            [Fact]
            public void SessionIsCalledWithBookmarks()
            {
                // Arrange
                var bookmarks = new List<string> {"Bookmark1", "Bookmark2"};

                var cypherQuery = new CypherQuery("RETURN 1", new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional) {Bookmarks = bookmarks};

                using (var testHarness = new BoltTestHarness())
                {
                    try
                    {
                        testHarness.CreateAndConnectBoltGraphClient().ExecuteGetCypherResults<object>(cypherQuery).ToArray();
                    }
                    catch
                    {
                        /*Not interested in actually getting results*/
                    }

                    //Assert
                    testHarness.MockDriver.Verify(d => d.Session(It.IsAny<AccessMode>(), It.Is<IEnumerable<string>>(s => s.Contains(bookmarks[0]) && s.Contains(bookmarks[1]))), Times.Once);
                }
            }
        }
    }
}