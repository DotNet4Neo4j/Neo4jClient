using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;

namespace Neo4jClient.Transactions
{
    using Neo4j.Driver;

    /// <summary>
    /// Handles all the queries related to transactions that could be needed in a ITransactionalGraphClient
    /// </summary>
    internal class TransactionManager : ITransactionManager<HttpResponseMessage>
    {
        // holds the transaction objects per thread
#if NET45
        [ThreadStatic] private static IScopedTransactions<TransactionScopeProxy> scopedTransactions;
        internal static IScopedTransactions<TransactionScopeProxy> ScopedTransactions
        {
            get => scopedTransactions ?? (scopedTransactions = ThreadContextHelper.CreateScopedTransactions());
            set => scopedTransactions = value;
        }
#else
        private static readonly AsyncLocal<IScopedTransactions<TransactionScopeProxy>> scopedTransactions
            = new AsyncLocal<IScopedTransactions<TransactionScopeProxy>>();
        internal static IScopedTransactions<TransactionScopeProxy> ScopedTransactions
        {
            get => scopedTransactions.Value ?? (scopedTransactions.Value = ThreadContextHelper.CreateScopedTransactions());
            set => scopedTransactions.Value = value;
        }
#endif

           
        private readonly ITransactionalGraphClient client;

        public TransactionManager(ITransactionalGraphClient client)
        {
            this.client = client;
            // specifies that we are about to use variables that depend on OS threads
            Thread.BeginThreadAffinity();
            ScopedTransactions = ThreadContextHelper.CreateScopedTransactions();
        }

        private TransactionContext GetContext(NameValueCollection customHeaders = null)
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
                //BUG: ??? Using System.Transactions here...?!?
                return Transaction.Current != null;
            }
        }

        public TransactionScopeProxy CurrentInternalTransaction => ScopedTransactions.TryPeek();

        public ITransaction CurrentTransaction => CurrentInternalTransaction;
        public Bookmark LastBookmark => CurrentTransaction.LastBookmark;

        public ITransaction BeginTransaction(TransactionScopeOption option, IEnumerable<string> bookmarks, string database)
        {
            return BeginTransaction(option, database);
        }

        // public ITransaction BeginTransaction(TransactionScopeOption scopeOption, IEnumerable<string> bookmarks)
        // {
        //     return BeginTransaction(scopeOption, client.DefaultDatabase);
        // }

        /// <summary>
        /// Implements the internal part for ITransactionalGraphClient.BeginTransaction
        /// </summary>
        /// <param name="scopeOption">How should the transaction scope be created.
        /// <see cref="ITransactionalGraphClient.BeginTransaction(TransactionScopeOption)"/> for more information.</param>
        /// <returns></returns>
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, string database)
        {
            if (scopeOption == TransactionScopeOption.Suppress)
            {
                // TransactionScopeOption.Suppress doesn't fail with older versions of Neo4j
                return BeginSuppressTransaction();
            }

            if (client.ServerVersion < new Version(2, 0))
            {
                throw new NotSupportedException("HTTP Transactions are only supported on Neo4j 2.0 and newer.");
            }

            if (scopeOption == TransactionScopeOption.Join)
            {
                var joinedTransaction = BeginJoinTransaction();
                if (joinedTransaction != null)
                {
                    return joinedTransaction;
                }
            }

            // then scopeOption == TransactionScopeOption.RequiresNew or we dont have a current transaction
            return BeginNewTransaction(database);
        }

        private TransactionContext GenerateTransaction(string database)
        {
            return new TransactionContext(new Neo4jRestTransaction(client, database));
        }

        private static TransactionContext GenerateTransaction(TransactionContext reference)
        {
            return new TransactionContext(reference.NeoTransaction);
        }

        private static void PushScopeTransaction(TransactionScopeProxy transaction)
        {
            ScopedTransactions.Push(transaction);
        }

        private ITransaction BeginNewTransaction(string database)
        {
            var transaction = new Neo4jTransactionProxy(client, GenerateTransaction(database), true);
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

            var joinedTransaction = new Neo4jTransactionProxy(client, GenerateTransaction(parentScope.TransactionContext), false);
            PushScopeTransaction(joinedTransaction);
            return joinedTransaction;
        }

        private ITransaction BeginSuppressTransaction()
        {
            var suppressTransaction = new SuppressTransactionProxy(client);
            PushScopeTransaction(suppressTransaction);
            return suppressTransaction;
        }


        

        public void EndTransaction()
        {
            var currentTransaction = ScopedTransactions.TryPop();
            currentTransaction?.Dispose();
        }

        public Task<HttpResponseMessage> EnqueueCypherRequest(string commandDescription, IGraphClient graphClient, CypherQuery query)
        {
            var policy = new CypherTransactionExecutionPolicy(graphClient);
            // we try to get the current dtc transaction. If we are in a System.Transactions transaction and it has
            // been "promoted" to be handled by DTC then transactionObject will be null, but it doesn't matter as
            // we don't care about updating the object.
            var txContext = GetContext(query.CustomHeaders);
            txContext.CustomHeaders = query.CustomHeaders;
            policy.Database = txContext.NeoTransaction.Database;
            // the main difference with a normal Request.With() call is that the request is associated with the
            // TX context.
            
            return txContext.EnqueueTask(commandDescription, graphClient, policy, query);
        }

        public void Dispose()
        {
            ScopedTransactions = null;
            Thread.EndThreadAffinity();
        }
    }
}