using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4jClient.Extensions;

namespace Neo4jClient.Transactions.Bolt
{
    internal class BoltNeo4jTransaction : ITransaction
    {
        internal readonly IAsyncTransaction DriverTransaction;
        internal IAsyncSession Session { get; }
        internal IList<string> Bookmarks { get; set; }
        public Guid Id { get; private set; }

        //TODO: Who uses this constructor??
        public BoltNeo4jTransaction(Version version, IDriver driver, IEnumerable<string> bookmarks, string database, bool isWrite = true)
        {
            Database = database;
            Bookmarks = bookmarks?.ToList();
            Session = driver.AsyncSession(version, database, isWrite, Bookmarks);

            var tx = Session.BeginTransactionAsync();
            tx.Wait();
            DriverTransaction = tx.Result;
            IsOpen = true;
            Id = Guid.NewGuid();
        }

        public BoltNeo4jTransaction(IAsyncSession session, IAsyncTransaction transaction, string database)
        {
            Database = database;
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
            
            if(IsOpen)
                RollbackAsync().Wait();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Implementation of ITransaction

        public string Database { get; set; }

        /// <inheritdoc />
        public async Task CommitAsync()
        {
            CheckOpenStatus();

            if (DriverTransaction != null)
                await DriverTransaction.CommitAsync();


            IsOpen = false;
        }

        private void CheckOpenStatus()
        {
            if(!IsOpen)
                throw new ClosedTransactionException(null);
        }

        /// <inheritdoc />
        public async Task RollbackAsync()
        {
            CheckOpenStatus();

            if (DriverTransaction != null)
                await DriverTransaction.RollbackAsync();

            IsOpen = false;
        }

        //TODO: Not needed
        /// <inheritdoc />
        #pragma warning disable 1998
        public async Task KeepAliveAsync() { }
        #pragma warning restore 1998

        /// <inheritdoc />
        public bool IsOpen { get; private set; }

        //TODO: Not needed
        /// <inheritdoc />
        public NameValueCollection CustomHeaders { get; set; }

        /// <summary>
        /// Gets the bookmark received following the last successfully completed Transaction.
        /// If no bookmark was received or if this transaction was rolled back, the bookmark value will not be changed. 
        /// </summary>
        public Bookmark LastBookmark => Session?.LastBookmark;

        #endregion
        
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

        // public static BoltNeo4jTransaction FromIdAndClient(Guid transactionId, IDriver driver)
        // {
        //     return new BoltNeo4jTransaction(driver, null, Database){Id = transactionId};
        // }
    }
}