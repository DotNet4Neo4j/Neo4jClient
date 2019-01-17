using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Neo4j.Driver.V1;

namespace Neo4jClient.Transactions
{
    internal class BoltNeo4jTransaction : ITransaction
    {
        internal readonly Neo4j.Driver.V1.ITransaction DriverTransaction;
        internal ISession Session { get; }
        internal IList<string> Bookmarks { get; set; }
        public Guid Id { get; private set; }

        public BoltNeo4jTransaction(IDriver driver, IEnumerable<string> bookmarks, bool isWrite = true)
        {
            Bookmarks = bookmarks?.ToList();
            Session = driver.Session(isWrite ? AccessMode.Write : AccessMode.Read, Bookmarks);
            DriverTransaction = Session.BeginTransaction();
            IsOpen = true;
            Id = Guid.NewGuid();
        }

        public BoltNeo4jTransaction(ISession session, Neo4j.Driver.V1.ITransaction transaction)
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
            DriverTransaction?.Dispose();
            Session?.Dispose();
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
        public void Commit()
        {
            DriverTransaction?.Success();
        }

        /// <inheritdoc />
        public void Rollback()
        {
            DriverTransaction?.Failure();
        }

        //TODO: Not needed
        /// <inheritdoc />
        public void KeepAlive()
        {
            /*Not needed for Bolt.*/
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
            transactionExecutionEnvironment.DriverTransaction.Success();
            transactionExecutionEnvironment.DriverTransaction.Dispose();
        }

        public static void DoRollback(ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment)
        {
            transactionExecutionEnvironment.DriverTransaction.Failure();
            transactionExecutionEnvironment.DriverTransaction.Dispose();
        }

        public static BoltNeo4jTransaction FromIdAndClient(Guid transactionId, IDriver driver)
        {
            return new BoltNeo4jTransaction(driver, null){Id = transactionId};
        }
    }
}