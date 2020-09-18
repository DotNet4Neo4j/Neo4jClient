using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;

namespace Neo4jClient.Transactions.Bolt
{
    /// <summary>
    /// Encapsulates a transaction object with its transaction scheduler.
    /// </summary>
    /// <remarks>
    /// All requests to the same transaction have to made sequentially. The purpose of this class is to ensure
    /// that such calls are made in that fashion.
    /// </remarks>
    internal class BoltTransactionContext : TransactionContextBase<BoltGraphClient, BoltResponse>
    {
        internal BoltNeo4jTransaction BoltTransaction => Transaction as BoltNeo4jTransaction;

        public BoltTransactionContext(ITransaction transaction) : base(transaction)
        {
        }

        protected override async Task<BoltResponse> RunQuery(BoltGraphClient graphClient, CypherQuery query,
            IExecutionPolicy policy, string commandDescription)
        {
            var result = await BoltTransaction.DriverTransaction.RunAsync(query, graphClient).ConfigureAwait(false);
            var resp = new BoltResponse {StatementResult = result};
            return resp;
        }
    }
}
