using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Neo4jClient.Cypher;
using Xunit;

namespace Neo4jClient.Tests.BoltGraphClientTests.Bookmarks
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
            public async Task TestMocking()
            {
                Mock<IAsyncSession> sessionMock = new Mock<IAsyncSession>();
                Mock<IDriver> driverMock = new Mock<IDriver>();
                driverMock.Setup(d => d.AsyncSession()).Returns(sessionMock.Object);
                driverMock.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(sessionMock.Object);

                driverMock.Object.AsyncSession(x => x.WithBookmarks(Bookmark.From("x"))).Should().NotBeNull();
            }

            [Fact]
            public async Task ArgsContainBookmarksUsed()
            {
                // Arrange
                var bookmarks = new List<Bookmark> {Bookmark.From("Bookmark1"), Bookmark.From("Bookmark2")};

                const string queryText = "RETURN [] AS data";

                var queryParams = new Dictionary<string, object>();

                var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j") {Bookmarks = bookmarks};

                using (var testHarness = new BoltTestHarness())
                {
                    var recordMock = new Mock<IRecord>();
                    recordMock.Setup(r => r["data"]).Returns(new List<INode>());
                    recordMock.Setup(r => r.Keys).Returns(new[] {"data"});

                    var testStatementResult = new TestStatementResult(new[] {"data"}, recordMock.Object);
                    testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                    var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                    graphClient.OperationCompleted += (s, e) =>
                    {
                        e.BookmarksUsed.Should().Contain(bookmarks[0]);
                        e.BookmarksUsed.Should().Contain(bookmarks[1]);
                    };

                    var driverSess = testHarness.MockDriver.Object.AsyncSession();
                    testHarness.MockDriver.Verify(s => s.AsyncSession(), Times.Once);
                    
                    driverSess.Should().NotBeNull();

                    await graphClient.ExecuteGetCypherResultsAsync<IEnumerable<ObjectWithIds>>(cypherQuery);
                }
            }


            [Fact]
            public async Task ArgsContainBookmarkUsed()
            {
                // Arrange
                var bookmark = Bookmark.From("Bookmark1");
                var bookmarks = new List<Bookmark> {bookmark};

                const string queryText = "RETURN [] AS data";

                var queryParams = new Dictionary<string, object>();

                var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j") {Bookmarks = bookmarks};

                using (var testHarness = new BoltTestHarness())
                {
                    var recordMock = new Mock<IRecord>();
                    recordMock.Setup(r => r["data"]).Returns(new List<INode>());
                    recordMock.Setup(r => r.Keys).Returns(new[] {"data"});

                    var testStatementResult = new TestStatementResult(new[] {"data"}, recordMock.Object);
                    testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                    var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                    graphClient.OperationCompleted += (s, e) => { e.BookmarksUsed.Should().Contain(bookmarks[0]); };

                    await graphClient.ExecuteGetCypherResultsAsync<IEnumerable<ObjectWithIds>>(cypherQuery);
                }
            }

            [Fact]
            public async Task ArgsContainLastBookmark()
            {
                var lastBookmark = Bookmark.From("LastBookmark");

                const string queryText = "RETURN [] AS data";

                var queryParams = new Dictionary<string, object>();

                var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j");

                using (var testHarness = new BoltTestHarness())
                {
                    var recordMock = new Mock<IRecord>();
                    recordMock.Setup(r => r["data"]).Returns(new List<INode>());
                    recordMock.Setup(r => r.Keys).Returns(new[] {"data"});

                    testHarness.MockSession.Setup(s => s.LastBookmark).Returns(lastBookmark);

                    var testStatementResult = new TestStatementResult(new[] {"data"}, recordMock.Object);
                    testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                    var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                    graphClient.OperationCompleted += (s, e) => { e.LastBookmark.Should().Be(lastBookmark); };

                    await graphClient.ExecuteGetCypherResultsAsync<IEnumerable<ObjectWithIds>>(cypherQuery);
                }
            }

            [Fact(Skip = "Can't test this at the moment, as there is no way to get the SessionConfigBuilder results")]
            public async Task SessionIsCalledWithBookmark()
            {
                // Arrange
                var bookmark = Bookmark.From("Bookmark1");

                var cypherQuery = new CypherQuery("RETURN 1", new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional, "neo4j") {Bookmarks = new List<Bookmark>{bookmark}};

                using (var testHarness = new BoltTestHarness())
                {
                    try
                    {
                        (await (await testHarness.CreateAndConnectBoltGraphClient()).ExecuteGetCypherResultsAsync<object>(cypherQuery)).ToArray();
                    }
                    catch
                    {
                        /*Not interested in actually getting results*/
                    }

                    Action<SessionConfigBuilder> scb;

                    //Assert
                    // testHarness.MockDriver.Verify(d => d.AsyncSession(It.IsAny<AccessMode>(), It.Is<IEnumerable<string>>(s => s.Contains(bookmark))), Times.Once);
                    // testHarness.MockDriver.Verify(d => d.AsyncSession(It.Is<Action<SessionConfigBuilder>>()));

                    // testHarness.MockDriver.Object.Config
                    testHarness.MockDriver.Verify(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>()), Times.Once);
                    throw new NotImplementedException();
                }
            }


            [Fact(Skip = "Can't test this at the moment, as there is no way to get the SessionConfigBuilder results")]
            public async Task SessionIsCalledWithBookmarks()
            {
                // Arrange
                var bookmarks = new List<Bookmark> { Bookmark.From("Bookmark1"), Bookmark.From("Bookmark2") };

                var cypherQuery = new CypherQuery("RETURN 1", new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional, "neo4j") {Bookmarks = bookmarks};

                using (var testHarness = new BoltTestHarness())
                {
                    try
                    {
                        (await (await testHarness.CreateAndConnectBoltGraphClient()).ExecuteGetCypherResultsAsync<object>(cypherQuery)).ToArray();
                    }
                    catch
                    {
                        /*Not interested in actually getting results*/
                    }

                    //Assert
                    // testHarness.MockDriver.Verify(d => d.Session(It.IsAny<AccessMode>(), It.Is<IEnumerable<string>>(s => s.Contains(bookmarks[0]) && s.Contains(bookmarks[1]))), Times.Once);
                    testHarness.MockDriver.Verify(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>()), Times.Once);

                    throw new NotImplementedException();
                }
            }
        }
    }
}