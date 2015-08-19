using System;

namespace Neo4jClient
{
    public delegate void OperationCompletedEventHandler(object sender, OperationCompletedEventArgs e);

    public class OperationCompletedEventArgs : EventArgs
    {
        public string QueryText { get; set; }
        public int ResourcesReturned { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public Exception Exception { get; set; }
        public bool HasException { get { return Exception != null; } }

        public override string ToString()
        {
            return string.Format("HasException={0}, QueryText={1}", HasException, QueryText);
        }
    }
}
