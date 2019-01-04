using System;

namespace Neo4jClient.Transactions
{
    internal class BoltSuppressTransactionProxy : BoltTransactionScopeProxy
    {
        public BoltSuppressTransactionProxy(ITransactionalGraphClient client)
            : base(client, null)
        {
        }

        protected override bool ShouldDisposeTransaction()
        {
            return false;
        }

        protected override void DoCommit()
        {
            throw new InvalidOperationException("Committing during a suppressed transaction scope");
        }

        public override bool Committable => false;

        public override void Rollback()
        {
            throw new InvalidOperationException("Rolling back during a suppressed transaction scope");
        }

        public override void KeepAlive()
        {
            // no-op
        }

        public override bool IsOpen => true;
    }
}