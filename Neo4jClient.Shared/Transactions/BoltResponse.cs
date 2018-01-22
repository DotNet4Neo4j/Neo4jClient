using Neo4j.Driver.V1;

namespace Neo4jClient.Transactions
{
    public class BoltResponse
    {
        public IStatementResult StatementResult { get; set; }
    }
}