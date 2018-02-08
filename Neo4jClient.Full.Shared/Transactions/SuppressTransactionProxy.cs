using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Transactions
{
    internal class SuppressTransactionProxy : TransactionScopeProxy
    {
        public SuppressTransactionProxy(ITransactionalGraphClient client)
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

        public override bool Committable
        {
            get { return false; }
        }

        public override void Rollback()
        {
            throw new InvalidOperationException("Rolling back during a suppressed transaction scope");
        }

        public override void KeepAlive()
        {
            // no-op
        }

        public override bool IsOpen
        {
            // we cannot call Commit() or Rollback() for this proxy
            get { return true; }
        }
    }
}
