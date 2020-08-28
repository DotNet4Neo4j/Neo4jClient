using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Neo4jClient.Transactions
{
    using Neo4j.Driver;

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
            get => transactionContext.Endpoint;
            set => transactionContext.Endpoint = value;
        }

        public NameValueCollection CustomHeaders { get; set; }
        public Bookmark LastBookmark => throw new InvalidOperationException("This is not possible with the GraphClient. You would need the BoltGraphClient.");

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
                RollbackAsync().Wait(); // annoying, but can't dispose asynchronously
            }

            if (transactionContext != null && ShouldDisposeTransaction())
            {
                transactionContext.Dispose();
                transactionContext = null;
            }
        }

        public string Database { get; set; }

        public Task CommitAsync()
        {
            markCommitted = true;
            if (CustomHeaders != null)
            {
                transactionContext.CustomHeaders = CustomHeaders;
            }
            return DoCommitAsync();
        }

        protected abstract bool ShouldDisposeTransaction();
        protected abstract Task DoCommitAsync();
        public abstract bool Committable { get; }
        public abstract Task RollbackAsync();
        public abstract Task KeepAliveAsync();
        public abstract bool IsOpen { get; }
    }
}
