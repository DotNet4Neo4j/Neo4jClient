using System;
using System.Threading.Tasks;

namespace Neo4jClient.Transactions.Bolt
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
#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        public override bool IsOpen => true;
    }
}