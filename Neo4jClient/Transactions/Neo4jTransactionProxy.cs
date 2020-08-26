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

        protected override void DoCommit()
        {
            if (doCommitInScope)
                TransactionContext.Commit();
        }

        protected override bool ShouldDisposeTransaction()
        {
            return doCommitInScope;
        }

        public override void Rollback()
        {
            TransactionContext.Rollback();
        }

        public override void KeepAlive()
        {
            TransactionContext.KeepAlive();
        }
    }
}