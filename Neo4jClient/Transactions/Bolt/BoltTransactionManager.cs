﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;

namespace Neo4jClient.Transactions
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
            get
            {
                if (scopedTransactions.Value == null)
                {
                    scopedTransactions.Value = ThreadContextHelper.CreateBoltScopedTransactions();
                }

                return scopedTransactions.Value;
            }
            set => scopedTransactions.Value = value;
        }
#endif

        // holds the transaction contexts for transactions from the System.Transactions framework
        private readonly IDictionary<string, BoltTransactionContext> dtcContexts;
        private readonly BoltTransactionPromotableSinglePhasesNotification promotable;
        private readonly ITransactionalGraphClient client;

        public BoltTransactionManager(ITransactionalGraphClient client)
        {
            this.client = client;
            // specifies that we are about to use variables that depend on OS threads
            Thread.BeginThreadAffinity();
            ScopedTransactions = ThreadContextHelper.CreateBoltScopedTransactions();

            // this object enables the interacion with System.Transactions and MSDTC, at first by
            // letting us manage the transaction objects ourselves, and if we require to be promoted to MSDTC,
            // then it notifies the library how to do it.
            promotable = new BoltTransactionPromotableSinglePhasesNotification(client);
            dtcContexts = new Dictionary<string, BoltTransactionContext>();
        }

        private BoltTransactionContext GetOrCreateDtcTransactionContext()
        {
            // we need to lock as we could get other async requests to the same transaction
            var txId = Transaction.Current.TransactionInformation.LocalIdentifier;
            lock (dtcContexts)
            {
                BoltTransactionContext txContext;
                if (dtcContexts.TryGetValue(txId, out txContext))
                {
                    return txContext;
                }

                // associate it with the ambient transaction
                txContext = new BoltTransactionContext(promotable.AmbientTransaction);
                dtcContexts[txId] = txContext;

                return txContext;
            }
        }

        private BoltTransactionContext GetContext()
        {
            var nonDtcTransaction = CurrentInternalTransaction;
            if (nonDtcTransaction != null && nonDtcTransaction.Committable)
            {
                return nonDtcTransaction.TransactionContext;
            }

            // if we are not in a native transaction get the context of our ambient transaction
            return GetOrCreateDtcTransactionContext();
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

        public BoltTransactionScopeProxy CurrentInternalTransaction
        {
            get
            {
                return ScopedTransactions.TryPeek();
            }
        }

        public ITransaction CurrentNonDtcTransaction => CurrentInternalTransaction;

        public ITransaction CurrentDtcTransaction => Transaction.Current == null ? null : promotable.AmbientTransaction;

        /// <summary>
        /// Implements the internal part for ITransactionalGraphClient.BeginTransaction
        /// </summary>
        /// <param name="scopeOption">How should the transaction scope be created.
        /// <see cref="Neo4jClient.Transactions.ITransactionalGraphClient.BeginTransaction(Neo4jClient.Transactions.TransactionScopeOption)" />
        ///  for more information.</param>
        /// <param name="bookmarks">Bookmarks for use with this transaction.</param>
        /// <returns></returns>
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption, IEnumerable<string> bookmarks)
        {
            if (scopeOption == TransactionScopeOption.Suppress)
            {
                // TransactionScopeOption.Suppress doesn't fail with older versions of Neo4j
                //TODO: Check this
                return BeginSupressTransaction();
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
            return BeginNewTransaction(bookmarks);
        }

        private BoltTransactionContext GenerateTransaction(IEnumerable<string> bookmarks)
        {
            var session = ((BoltGraphClient)client).Driver.Session(bookmarks);
            var transaction = session.BeginTransaction();
            return new BoltTransactionContext(new BoltNeo4jTransaction(session, transaction));
        }

        private BoltTransactionContext GenerateTransaction(BoltTransactionContext reference)
        {
            return new BoltTransactionContext(reference.Transaction);
        }

        private void PushScopeTransaction(BoltTransactionScopeProxy transaction)
        {
            ScopedTransactions.Push(transaction);
        }

        private ITransaction BeginNewTransaction(IEnumerable<string> bookmarks)
        {
            var transaction = new BoltNeo4jTransactionProxy(client, GenerateTransaction(bookmarks), true);
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

        private ITransaction BeginSupressTransaction()
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

        /// <summary>
        /// Registers to ambient System.Transactions.TransactionContext if needed
        /// </summary>
        public void RegisterToTransactionIfNeeded()
        {
            //If promotable is null - we don't support tx.
            promotable?.EnlistIfNecessary();
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