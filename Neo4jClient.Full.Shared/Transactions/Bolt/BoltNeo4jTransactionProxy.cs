using Neo4j.Driver.V1;

namespace Neo4jClient.Transactions
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