using System.Threading.Tasks;

namespace Neo4jClient.Transactions.Bolt
{
    /// <summary>
    ///     Implements the TransactionScopeProxy interfaces for INeo4jTransaction
    /// </summary>
    internal class BoltNeo4jTransactionProxy : BoltTransactionScopeProxy
    {
        private readonly bool doCommitInScope;

        public BoltNeo4jTransactionProxy(ITransactionalGraphClient client, BoltTransactionContext transactionContext, bool newScope)
            : base(client, transactionContext)
        {
            doCommitInScope = newScope;

        }

        public override bool Committable => true;

        public override bool IsOpen => (TransactionContext != null) && TransactionContext.IsOpen;

        protected override Task DoCommitAsync()
        {
            return doCommitInScope ? TransactionContext.CommitAsync() : Task.CompletedTask;
        }

        protected override bool ShouldDisposeTransaction()
        {
            return doCommitInScope;
        }

        public override Task RollbackAsync()
        {
            return TransactionContext.RollbackAsync();
        }

        public override Task KeepAliveAsync()
        {
            return TransactionContext.KeepAliveAsync();
        }
    }
}