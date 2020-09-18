using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Neo4jClient.Cypher;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class WithBookmarkMethod : IClassFixture<CultureInfoSetupFixture>
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void DoesNothing_WhenBookmarkIsWhitespaceOrNull(string bookmark)
        {
            var mockGc = new Mock<IRawGraphClient>();

            var cfq = new CypherFluentQuery(mockGc.Object);
            cfq.WithBookmark(bookmark);
            var query = cfq.Query;

            query.Bookmarks.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void DoesNothing_WhenBookmarkIsWhitespaceOrNull_ForBookmarks(string bookmark)
        {
            var mockGc = new Mock<IRawGraphClient>();
            var list = new List<string> { bookmark };
            var cfq = new CypherFluentQuery(mockGc.Object);
            cfq.WithBookmarks(list.ToArray());
            var query = cfq.Query;

            query.Bookmarks.Should().HaveCount(0);
        }

        [Fact]
        public void SetsBookmark_InQuery()
        {
            const string bookmarkName = "Bookmark1";
            var mockGc = new Mock<IRawGraphClient>();

            var cfq = new CypherFluentQuery(mockGc.Object);
            cfq.WithBookmark(bookmarkName);
            var query = cfq.Query;

            query.Bookmarks.Should().HaveCount(1);
            query.Bookmarks.SelectMany(b => b.Values).Should().Contain(bookmarkName);
        }

        [Fact]
        public void SetsBookmarks_InQuery1()
        {
            const string bookmarkName1 = "Bookmark1";
            const string bookmarkName2 = "Bookmark2";
            var mockGc = new Mock<IRawGraphClient>();

            var cfq = new CypherFluentQuery(mockGc.Object);
            cfq.WithBookmarks(bookmarkName1, bookmarkName2);
            var query = cfq.Query;

            query.Bookmarks.Should().HaveCount(2);
            var bmarks = query.Bookmarks.SelectMany(b => b.Values).ToList();
            bmarks.Should().Contain(bookmarkName1);
            bmarks.Should().Contain(bookmarkName2);
        }

        [Fact]
        public void SetsBookmarks_InQuery2()
        {
            const string bookmarkName1 = "Bookmark1";
            const string bookmarkName2 = "Bookmark2";
            var list = new List<string> { bookmarkName1, bookmarkName2 };
            var mockGc = new Mock<IRawGraphClient>();

            var cfq = new CypherFluentQuery(mockGc.Object);
            cfq.WithBookmarks(list.ToArray());
            var query = cfq.Query;

            query.Bookmarks.Should().HaveCount(2);
            var bmarks = query.Bookmarks.SelectMany(b => b.Values).ToList();
            bmarks.Should().Contain(bookmarkName1);
            bmarks.Should().Contain(bookmarkName2);
        }
    }
}
