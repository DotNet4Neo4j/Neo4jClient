using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Neo4j.Driver;
using Neo4jClient.ApiModels.Cypher;

namespace Neo4jClient
{
    public delegate void OperationCompletedEventHandler(object sender, OperationCompletedEventArgs e);

    public class OperationCompletedEventArgs : EventArgs
    {
        public string Database { get; set; }
        public string Identifier { get; set; }
        [Obsolete("Replaced with 'LastBookmarks' will be removed in the next version.")]
        public Bookmark LastBookmark { get; set; }
        public Bookmarks LastBookmarks { get; set; }
        public string QueryText { get; set; }
        public int ResourcesReturned { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public Exception Exception { get; set; }
        public bool HasException => Exception != null;
        public int? MaxExecutionTime { get; set; }
        public NameValueCollection CustomHeaders { get; set; }

        public IEnumerable<Bookmark> BookmarksUsed { get; set; }
        public QueryStats QueryStats { get; set; }

        /// <summary>This is only set with the <see cref="BoltGraphClient" />.</summary>
        public Config ConfigUsed { get; set; }

        public override string ToString()
        {
            return $"HasException={HasException}, QueryText={QueryText}";
        }
    }
}