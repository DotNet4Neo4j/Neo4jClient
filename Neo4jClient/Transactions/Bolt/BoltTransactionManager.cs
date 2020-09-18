using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Neo4j.Driver;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Extensions;

namespace Neo4jClient.Transactions.Bolt
{
    /// <summary>
    /// Handles all the queries related to transactions that could be needed in a ITransactionalGraphClient
    /// </summary>
    internal class BoltTransactionManager : ITransactionManager<BoltResponse>
    {
#if NET45
        // holds the transaction objects per thread
        [ThreadStatic] private static IScopedTransactions<BoltTransactionScopeProxy> scopedTransactions;
        internal static IScopedTransactions<BoltTransactionScopeProxy> ScopedTransactions
        {
            get => scopedTransactions ?? (scopedTransactions = ThreadContextHelper.CreateBoltScopedTransactions());
            set => scopedTransactions = value;
        }
#else
        private static readonly AsyncLocal<IScopedTransactions<BoltTransactionScopeProxy>> scopedTransactions
            = new AsyncLocal<IScopedTransactions<BoltTransactionScopeProxy>>();

        internal static IScopedTransactions<BoltTransactionScopeProxy> ScopedTransactions
        {
            get => scopedTransactions.Value ?? (scopedTransactions.Value = ThreadContextHelper.CreateBoltScopedTransactions());
            set => scopedTransactions.Value = value;
        }
#endif

        private readonly ITransactionalGraphClient client;

        public BoltTransactionManager(ITransactionalGraphClient client)
        {
            this.client = client;
            // specifies that we are about to use variables that depend on OS threads
            Thread.BeginThreadAffinity();
            ScopedTransactions = ThreadContextHelper.CreateBoltScopedTransactions();
        }

        private BoltTransactionContext GetContext()
        {
            var nonDtcTransaction = CurrentInternalTransaction;
            if (nonDtcTransaction != null && nonDtcTransaction.Committable)
            {
                return nonDtcTransaction.TransactionContext;
            }

            throw new InvalidOperationException("There is no active transaction");
        }

        public bool InTransaction
        {
            get
            {
                var transactionObject = CurrentInternalTransaction;
                if (transactionObject != null)
                {
                    return transactionObject.Committable;
                }

                // if we are in an ambient System.Transactions transaction then we are in a transaction!
                return Transaction.Current != null;
            }
        }

        public BoltTransactionScopeProxy CurrentInternalTransaction => ScopedTransactions.TryPeek();

        public ITransaction CurrentTransaction => CurrentInternalTransaction;

        public Bookmark LastBookmark => CurrentTransaction.LastBookmark;

        /// <inheritdoc cref="ITransactionManager{T}.BeginTransaction"/>
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, IEnumerable<string> bookmarks, string database)
        {
            if (scopeOption == TransactionScopeOption.Suppress)
            {
                // TransactionScopeOption.Suppress doesn't fail with older versions of Neo4j
                //TODO: Check this
                return BeginSuppressTransaction();
            }

            if (client.ServerVersion < new Version(3, 0))
            {
                throw new NotSupportedException("Bolt Transactions are only supported on Neo4j 3.0 and newer.");
            }

            //TODO: Check this
            if (scopeOption == TransactionScopeOption.Join)
            {
                var joinedTransaction = BeginJoinTransaction();
                if (joinedTransaction != null)
                {
                    return joinedTransaction;
                }
            }

            // then scopeOption == TransactionScopeOption.RequiresNew or we dont have a current transaction
            return BeginNewTransaction(bookmarks, database);
        }

        private BoltTransactionContext GenerateTransaction(IEnumerable<string> bookmarks, string database)
        {
            var session = ((BoltGraphClient) client).Driver.AsyncSession(client.ServerVersion, database, true, bookmarks);
            var transactionTask = session.BeginTransactionAsync();
            transactionTask.Wait();
            var transaction = transactionTask.Result;
            return new BoltTransactionContext(new BoltNeo4jTransaction(session, transaction, database));
        }

        private BoltTransactionContext GenerateTransaction(BoltTransactionContext reference)
        {
            return new BoltTransactionContext(reference.Transaction);
        }

        private void PushScopeTransaction(BoltTransactionScopeProxy transaction)
        {
            ScopedTransactions.Push(transaction);
        }

        private ITransaction BeginNewTransaction(IEnumerable<string> bookmarks, string database)
        {
            var transaction = new BoltNeo4jTransactionProxy(client, GenerateTransaction(bookmarks, database), true);
            PushScopeTransaction(transaction);
            return transaction;
        }

        private ITransaction BeginJoinTransaction()
        {
            var parentScope = CurrentInternalTransaction;
            if (parentScope == null)
            {
                return null;
            }

            if (!parentScope.Committable)
            {
                return null;
            }

            if (!parentScope.IsOpen)
            {
                throw new ClosedTransactionException(null);
            }

            var joinedTransaction = new BoltNeo4jTransactionProxy(client, GenerateTransaction(parentScope.TransactionContext), false);
            PushScopeTransaction(joinedTransaction);
            return joinedTransaction;
        }

        private ITransaction BeginSuppressTransaction()
        {
            var suppressTransaction = new BoltSuppressTransactionProxy(client);
            PushScopeTransaction(suppressTransaction);
            return suppressTransaction;
        }

        public void EndTransaction()
        {
            var currentTransaction = ScopedTransactions.TryPop();
            currentTransaction?.Dispose();
        }

        public void Dispose()
        {
            ScopedTransactions = null;
            Thread.EndThreadAffinity();
        }

        #region Implementation of ITransactionManager

        public Task<BoltResponse> EnqueueCypherRequest(string commandDescription, IGraphClient graphClient, CypherQuery query)
        {
            var policy = new CypherTransactionExecutionPolicy(graphClient);
            // we try to get the current dtc transaction. If we are in a System.Transactions transaction and it has
            // been "promoted" to be handled by DTC then transactionObject will be null, but it doesn't matter as
            // we don't care about updating the object.
            var txContext = GetContext();
            // the main difference with a normal Request.With() call is that the request is associated with the
            // TX context.
            return txContext.EnqueueTask(commandDescription, client as BoltGraphClient, policy, query);
        }

        #endregion
    }
}