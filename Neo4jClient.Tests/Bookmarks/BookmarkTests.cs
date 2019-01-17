using System.Collections.Generic;
using System.Linq;
using Moq;
using Neo4jClient.Test.Fixtures;
using Neo4jClient.Transactions;
using Xunit;

namespace Neo4jClient.Test.BoltGraphClientTests
{
    public class FullBookmarkTests : IClassFixture<CultureInfoSetupFixture>
    {
        public class OperationCompletedEvent : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void TransactionIsCalledWithBookmark()
            {
                // Arrange
                const string bookmark = "Bookmark1";
                var bookmarks = new List<string> {bookmark};

                using (var testHarness = new BoltTestHarness())
                {
                    var gc = testHarness.CreateAndConnectBoltGraphClient() as ITransactionalGraphClient;
                    gc.BeginTransaction(bookmarks);

                    //Assert
                    testHarness.MockDriver.Verify(d => d.Session(It.Is<IEnumerable<string>>(s => s.Contains(bookmark))), Times.Once);
                }
            }

            [Fact]
            public void TransactionIsCalledWithBookmarks()
            {
                // Arrange
                var bookmarks = new List<string> {"Bookmark1", "Bookmark2"};

                using (var testHarness = new BoltTestHarness())
                {
                    var gc = testHarness.CreateAndConnectBoltGraphClient() as ITransactionalGraphClient;
                    gc.BeginTransaction(bookmarks);

                    //Assert
                    testHarness.MockDriver.Verify(d => d.Session(It.Is<IEnumerable<string>>(s => s.Contains(bookmarks[0]) && s.Contains(bookmarks[1]))), Times.Once);
                }
            }
        }
    }
}