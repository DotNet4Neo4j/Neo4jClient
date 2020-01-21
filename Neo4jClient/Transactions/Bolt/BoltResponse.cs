using Neo4j.Driver;

namespace Neo4jClient.Transactions.Bolt
{
    public class BoltResponse
    {
        public IResultCursor StatementResult { get; set; }
    }
}