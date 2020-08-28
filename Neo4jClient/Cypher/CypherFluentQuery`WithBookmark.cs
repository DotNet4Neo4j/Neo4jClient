using System.Linq;
using Neo4j.Driver;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        /// <inheritdoc />
        public ICypherFluentQuery WithBookmark(string bookmark)
        {
            if (string.IsNullOrWhiteSpace(bookmark))
                return this;

            QueryWriter.Bookmarks.Add(Bookmark.From(bookmark));
            return this;
        }

        /// <inheritdoc />
        public ICypherFluentQuery WithBookmarks(params string[] bookmarks)
        {
            if (bookmarks == null)
                return this;

            QueryWriter.Bookmarks.AddRange(bookmarks.Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => Bookmark.From(b)));
            return this;
        }
    }
}