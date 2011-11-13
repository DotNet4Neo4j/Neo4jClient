using System;

namespace Neo4jClient
{
    public delegate void OperationCompletedEventHandler(object sender, EventArgs e);

    public class QueryCompletedEventArgs : EventArgs
    {
        public string QueryText { get; set; }
        public int ResourcesReturned { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }
}
