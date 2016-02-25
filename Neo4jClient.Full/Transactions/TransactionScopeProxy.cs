using System;
using System.Collections.Specialized;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Represents a TransactionContext scope within an ITransactionalManager. Encapsulates the real TransactionContext, so that in reality
    /// it only exists one single TransactionContext object in a joined scope, but multiple TransactionScopeProxies that can be pushed, or
    /// popped (in a scope context).
    /// </summary>
    internal abstract class TransactionScopeProxy : INeo4jTransaction
    {
        private readonly ITransactionalGraphClient _client;
        private bool _markCommitted = false;
        private bool _disposing = false;
        private TransactionContext _transactionContext;


        public TransactionContext TransactionContext
        {
            get { return _transactionContext; }
        }

        protected TransactionScopeProxy(ITransactionalGraphClient client, TransactionContext transactionContext)
        {
            _client = client;
            _disposing = false;
            _transactionContext = transactionContext;
        }

        public Uri Endpoint
        {
            get { return _transactionContext.Endpoint; }
            set { _transactionContext.Endpoint = value; }
        }

        public NameValueCollection CustomHeaders { get; set; }

        public virtual void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;
            _client.EndTransaction();
            if (!_markCommitted && Committable && TransactionContext.IsOpen)
            {
                Rollback();
            }

            if (_transactionContext != null && ShouldDisposeTransaction())
            {
                _transactionContext.Dispose();
                _transactionContext = null;
            }
        }

        public void Commit()
        {
            _markCommitted = true;
            if (CustomHeaders != null)
            {
                _transactionContext.CustomHeaders = CustomHeaders;
            }
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
