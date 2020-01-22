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
            public async Task ArgsContainBookmarksUsed()
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

                    var graphClient = await testHarness.CreateAndConnectBoltGraphClient();
                    graphClient.OperationCompleted += (s, e) =>
                    {
                        e.BookmarksUsed.Should().Contain(bookmarks[0]);
                        e.BookmarksUsed.Should().Contain(bookmarks[1]);
                    };

                    await graphClient.ExecuteGetCypherResultsAsync<IEnumerable<ObjectWithIds>>(cypherQuery);
                }
            }


            [Fact]
            public async Task ArgsContainBookmarkUsed()
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

                var cypherQuery = new CypherQuery(queryText, queryParams, CypherResultMode.Set, CypherResultFormat.Transactional);

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

            [Fact]
            public async Task SessionIsCalledWithBookmark()
            {
                // Arrange
                var bookmark = Bookmark.From("Bookmark1");

                var cypherQuery = new CypherQuery("RETURN 1", new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional) {Bookmarks = new List<string>(bookmark.Values)};

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


            [Fact]
            public async Task SessionIsCalledWithBookmark_TEST()
            {
                // Arrange
                var bookmark = Bookmark.From("Bookmark1");
                
                var cypherQuery = new CypherQuery("RETURN 1", new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional) { Bookmarks = new List<string>(bookmark.Values) };

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
                    // testHarness.MockDriver.Verify(d => d.AsyncSession(It.IsAny<AccessMode>(), It.Is<IEnumerable<string>>(s => s.Contains(bookmark))), Times.Once);
                    
                    // testHarness.MockDriver.Object.Config
                    testHarness.MockDriver.Verify(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>()), Times.Once);
                    throw new NotImplementedException();
                }
            }

            [Fact]
            public async Task SessionIsCalledWithBookmarks()
            {
                // Arrange
                var bookmarks = new List<string> {"Bookmark1", "Bookmark2"};

                var cypherQuery = new CypherQuery("RETURN 1", new Dictionary<string, object>(), CypherResultMode.Projection, CypherResultFormat.Transactional) {Bookmarks = bookmarks};

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