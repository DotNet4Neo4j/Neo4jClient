using System.Threading.Tasks;

namespace Neo4jClient.Transactions
{
    /// <summary>
    ///     Implements the TransactionScopeProxy interfaces for INeo4jTransaction
    /// </summary>
    internal class Neo4jTransactionProxy : TransactionScopeProxy
    {
        private readonly bool doCommitInScope;

        public Neo4jTransactionProxy(ITransactionalGraphClient client, TransactionContext transactionContext, bool newScope)
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