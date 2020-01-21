using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Neo4jClient.Transactions.Bolt
{
    internal class BoltNeo4jTransaction : ITransaction
    {
        internal readonly IAsyncTransaction DriverTransaction;
        internal IAsyncSession Session { get; }
        internal IList<string> Bookmarks { get; set; }
        public Guid Id { get; private set; }

        public BoltNeo4jTransaction(IDriver driver, IEnumerable<string> bookmarks, bool isWrite = true)
        {
            Bookmarks = bookmarks?.ToList();
            var accessMode = isWrite ? AccessMode.Write : AccessMode.Read;
            Session = driver.AsyncSession(x => x.WithDefaultAccessMode(accessMode).WithBookmarks(Bookmark.From(Bookmarks.ToArray())));

            var tx = Session.BeginTransactionAsync();
            tx.Wait();
            DriverTransaction = tx.Result;
            IsOpen = true;
            Id = Guid.NewGuid();
        }

        public BoltNeo4jTransaction(IAsyncSession session, IAsyncTransaction transaction)
        {
            DriverTransaction = transaction;
            Session = session;
            IsOpen = true;
            Id = Guid.NewGuid();
        }

        #region Implementation of IDisposable

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing)
                return;

            IsOpen = false;
            // DriverTransaction?
            // Session?.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Implementation of ITransaction

        /// <inheritdoc />
        public Task CommitAsync()
        {
            var task = DriverTransaction?.CommitAsync();
            if (task != null)
            {
                return task;
            }

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        /// <inheritdoc />
        public Task RollbackAsync()
        {
            var task = DriverTransaction?.RollbackAsync();
            if (task != null)
            {
                return task;
            }

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        //TODO: Not needed
        /// <inheritdoc />
        public Task KeepAliveAsync()
        {
            /*Not needed for Bolt.*/
#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        /// <inheritdoc />
        public bool IsOpen { get; private set; }

        //TODO: Not needed
        /// <inheritdoc />
        public NameValueCollection CustomHeaders { get; set; }

        #endregion

        
        /// <summary>
        /// Cancels a transaction without closing it in the server
        /// </summary>
        internal void Cancel()
        {
            IsOpen = false;
        }

        public static void DoCommit(ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment)
        {
            transactionExecutionEnvironment.DriverTransaction.CommitAsync().Wait();
            // transactionExecutionEnvironment.DriverTransaction.Dispose();
        }

        public static void DoRollback(ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment)
        {
            transactionExecutionEnvironment.DriverTransaction.RollbackAsync().Wait();
            // transactionExecutionEnvironment.DriverTransaction.Dispose();
        }

        public static BoltNeo4jTransaction FromIdAndClient(Guid transactionId, IDriver driver)
        {
            return new BoltNeo4jTransaction(driver, null){Id = transactionId};
        }
    }
}