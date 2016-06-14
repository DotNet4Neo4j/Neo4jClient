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

        public Neo4jTransactionProxy(ITransactionalGraphClient client, TransactionContext transactionContext, bool newScope)
            : base(client, transactionContext)
        {
            _doCommitInScope = newScope;
        }

        protected override void DoCommit()
        {
            if (_doCommitInScope)
            {
                TransactionContext.Commit();
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
            TransactionContext.Rollback();
        }

        public override void KeepAlive()
        {
            TransactionContext.KeepAlive();
        }

        public override bool IsOpen
        {
            get
            {
                return TransactionContext != null && TransactionContext.IsOpen;
            }
        }
    }
}
