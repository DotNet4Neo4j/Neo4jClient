using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Neo4jClient
{
    public delegate void OperationCompletedEventHandler(object sender, OperationCompletedEventArgs e);

    public class OperationCompletedEventArgs : EventArgs
    {
        public string Identifier { get; set; }
        public string LastBookmark { get; set; }
        public string QueryText { get; set; }
        public int ResourcesReturned { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public Exception Exception { get; set; }
        public bool HasException { get { return Exception != null; } }
        public int? MaxExecutionTime { get; set; }
        public NameValueCollection CustomHeaders { get; set; }
        public IEnumerable<string> BookmarksUsed { get; set; }

        public override string ToString()
        {
            return $"HasException={HasException}, QueryText={QueryText}";
        }
    }
}
