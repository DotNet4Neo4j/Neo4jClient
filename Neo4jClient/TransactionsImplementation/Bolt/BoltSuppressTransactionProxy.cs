using System;
using System.Threading.Tasks;

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

        protected override Task DoCommitAsync()
        {
            throw new InvalidOperationException("Committing during a suppressed transaction scope");
        }

        public override bool Committable => false;

        public override Task RollbackAsync()
        {
            throw new InvalidOperationException("Rolling back during a suppressed transaction scope");
        }

        public override Task KeepAliveAsync()
        {
            // no-op
            return Task.CompletedTask;
        }

        public override bool IsOpen => true;
    }
}