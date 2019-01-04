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
        private readonly ITransactionalGraphClient client;
        private bool markCommitted = false;
        private bool disposing = false;
        private TransactionContext transactionContext;


        public TransactionContext TransactionContext => transactionContext;

        protected TransactionScopeProxy(ITransactionalGraphClient client, TransactionContext transactionContext)
        {
            this.client = client;
            disposing = false;
            this.transactionContext = transactionContext;
        }

        public Uri Endpoint
        {
            get { return transactionContext.Endpoint; }
            set { transactionContext.Endpoint = value; }
        }

        public NameValueCollection CustomHeaders { get; set; }

        public virtual void Dispose()
        {
            if (disposing)
            {
                return;
            }

            disposing = true;
            client.EndTransaction();
            if (!markCommitted && Committable && TransactionContext.IsOpen)
            {
                Rollback();
            }

            if (transactionContext != null && ShouldDisposeTransaction())
            {
                transactionContext.Dispose();
                transactionContext = null;
            }
        }

        public void Commit()
        {
            markCommitted = true;
            if (CustomHeaders != null)
            {
                transactionContext.CustomHeaders = CustomHeaders;
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
