using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Transactions;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Represents a transaction scope within an ITransactionalManager. Encapsulates the real transaction, so that in reality
    /// it only exists one single transaction object in a joined scope, but multiple TransactionScopeProxies that can be pushed, or
    /// popped (in a scope context).
    /// </summary>
    internal abstract class TransactionScopeProxy : INeo4jTransaction
    {
        private readonly ITransactionalGraphClient _client;
        private bool _markCommitted = false;
        private bool _disposing = false;
        private INeo4jTransaction _transaction;


        public ITransaction Transaction
        {
            get { return _transaction; }
        }

        protected TransactionScopeProxy(ITransactionalGraphClient client, INeo4jTransaction transaction)
        {
            _client = client;
            _disposing = false;
            _transaction = transaction;
        }

        public Uri Endpoint
        {
            get { return _transaction.Endpoint; }
            set { _transaction.Endpoint = value; }
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

            if (_transaction != null && ShouldDisposeTransaction())
            {
                _transaction.Dispose();
                _transaction = null;
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
