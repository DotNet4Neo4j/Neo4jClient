using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Implements the TransactionScopeProxy interfaces for INeo4jTransaction
    /// </summary>
    internal class Neo4jTransactionProxy : TransactionScopeProxy
    {
        private readonly bool _doCommitInScope;

        public Neo4jTransactionProxy(ITransactionalGraphClient client, INeo4jTransaction transaction, bool newScope)
            : base(client, transaction)
        {
            _doCommitInScope = newScope;
        }

        protected override void DoCommit()
        {
            if (_doCommitInScope)
            {
                Transaction.Commit();
            }
        }

        protected override bool ShouldDisposeTransaction()
        {
            return _doCommitInScope;
        }

        public override bool Committable
        {
            get { return true; }
        }

        public override void Rollback()
        {
            Transaction.Rollback();
        }

        public override void KeepAlive()
        {
            Transaction.KeepAlive();
        }

        public override bool IsOpen
        {
            get
            {
                return Transaction != null && Transaction.IsOpen;
            }
        }
    }
}
