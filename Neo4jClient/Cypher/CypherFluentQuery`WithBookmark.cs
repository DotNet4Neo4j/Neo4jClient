using System.Linq;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        /// <inheritdoc />
        public ICypherFluentQuery WithBookmark(string bookmark)
        {
            if (string.IsNullOrWhiteSpace(bookmark))
                return this;

            QueryWriter.Bookmarks.Add(bookmark);
            return this;
        }

        /// <inheritdoc />
        public ICypherFluentQuery WithBookmarks(params string[] bookmarks)
        {
            QueryWriter.Bookmarks.AddRange(bookmarks.Where(b => !string.IsNullOrWhiteSpace(b)));
            return this;
        }
    }
}