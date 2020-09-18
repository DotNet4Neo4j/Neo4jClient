using System.Threading.Tasks;

namespace Neo4jClient.Transactions.Bolt
{
    using Neo4j.Driver;

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

        public override bool IsOpen => TransactionContext != null && TransactionContext.IsOpen;
        public override Bookmark LastBookmark => TransactionContext?.BoltTransaction?.LastBookmark;

        protected override async Task DoCommitAsync()
        {
            if (doCommitInScope)
                await TransactionContext.CommitAsync().ConfigureAwait(false);
        }

        public override string Database { get; set; }

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