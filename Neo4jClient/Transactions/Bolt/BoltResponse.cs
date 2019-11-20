using Neo4j.Driver.V1;

namespace Neo4jClient.Transactions.Bolt
{
    public class BoltResponse
    {
        public IStatementResultCursor StatementResult { get; set; }
    }
}