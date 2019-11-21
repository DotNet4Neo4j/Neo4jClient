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
            if (doCommitInScope)
                return TransactionContext.CommitAsync();
#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
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