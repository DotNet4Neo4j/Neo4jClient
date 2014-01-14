using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Transactions;

namespace Neo4jClient.Transactions
{
    internal abstract class TransactionScopeProxy : ITransaction
    {
        private ITransactionalGraphClient _client;
        private bool _markCommitted = false;
        private bool _disposing = false;


        public ITransaction Transaction { get; private set; }

        protected TransactionScopeProxy(ITransactionalGraphClient client, ITransaction transaction)
        {
            _client = client;
            _disposing = false;
            Transaction = transaction;
        }

        public virtual void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;
            _client.EndTransaction();
            if (!_markCommitted && Committable && Transaction.IsOpen)
            {
                Rollback();
            }

            if (Transaction != null && ShouldDisposeTransaction())
            {
                Transaction.Dispose();
                Transaction = null;
            }
        }

        public void Commit()
        {
            _markCommitted = true;
            DoCommit();
        }

        protected abstract bool ShouldDisposeTransaction();
        protected abstract void DoCommit();
        public abstract bool Committable { get; }
        public abstract void Rollback();
        public abstract void KeepAlive();
        public abstract bool IsOpen { get; }
    }
}
