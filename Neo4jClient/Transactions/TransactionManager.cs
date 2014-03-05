﻿using System;
using System.Collections.Generic;
using System.Net.Http;
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
    internal class TransactionManager : ITransactionManager
    {
        // holds the transaction objects per thread
        [ThreadStatic] private static Stack<TransactionScopeProxy> _scopedTransactions;
        // holds the transaction contexts for transactions from the System.Transactions framework
        private IDictionary<string, TransactionContext> _dtcContexts; 
        private TransactionPromotableSinglePhaseNotification _promotable;
        private ITransactionalGraphClient _client;

        public TransactionManager(ITransactionalGraphClient client)
        {
            _client = client;
            // specifies that we are about to use variables that depend on OS threads
            Thread.BeginThreadAffinity();
            _scopedTransactions = new Stack<TransactionScopeProxy>();

            // this object enables the interacion with System.Transactions and MSDTC, at first by
            // letting us manage the transaction objects ourselves, and if we require to be promoted to MSDTC,
            // then it notifies the library how to do it.
            _promotable = new TransactionPromotableSinglePhaseNotification(client);
            _dtcContexts = new Dictionary<string, TransactionContext>();
        }

        private TransactionContext GetOrCreateDtcTransactionContext()
        {
            // we need to lock as we could get other async requests to the same transaction
            var txId = Transaction.Current.TransactionInformation.LocalIdentifier;
            lock (_dtcContexts)
            {
                TransactionContext txContext;
                if (_dtcContexts.TryGetValue(txId, out txContext))
                {
                    return txContext;
                }

                // associate it with the ambient transaction
                txContext = new TransactionContext(_promotable.AmbientTransaction);
                _dtcContexts[txId] = txContext;

                return txContext;
            }
        }

        private TransactionContext GetContext()
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
                // if we are in an ambient System.Transactions transaction then we are in a transaction!
                if (Transaction.Current != null)
                {
                    return true;
                }

                var transactionObject = CurrentInternalTransaction;
                return transactionObject != null && transactionObject.Committable;
            }
        }

        public TransactionScopeProxy CurrentInternalTransaction
        {
            get
            {
                try
                {
                    return _scopedTransactions == null ? null : _scopedTransactions.Peek();
                }
                catch (InvalidOperationException)
                {
                    // the stack is empty
                    return null;
                }
            }
        }

        public ITransaction CurrentNonDtcTransaction
        {
            get { return CurrentInternalTransaction; }
        }

        public ITransaction CurrentDtcTransaction
        {
            get
            {
                return Transaction.Current == null ? null : _promotable.AmbientTransaction;
            }
        }

        /// <summary>
        /// Implements the internal part for ITransactionalGraphClient.BeginTransaction
        /// </summary>
        /// <param name="option">How should the transaction scope be created.
        /// <see cref="Neo4jClient.Transactions.ITransactionalGraphClient.BeginTransaction(Neo4jClient.Transactions.TransactionScopeOption)" />
        ///  for more information.</param>
        /// <returns></returns>
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption)
        {
            if (scopeOption == TransactionScopeOption.Suppress)
            {
                // TransactionScopeOption.Suppress doesn't fail with older versions of Neo4j
                return BeginSupressTransaction();
            }

            if (_client.ServerVersion < new Version(2, 0))
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
            return BeginNewTransaction();
        }

        private TransactionContext GenerateTransaction()
        {
            return new TransactionContext(new Neo4jTransaction(_client));
        }

        private TransactionContext GenerateTransaction(TransactionContext reference)
        {
            return new TransactionContext(reference.Transaction);
        }

        private void PushScopeTransaction(TransactionScopeProxy transaction)
        {
            if (_scopedTransactions == null)
            {
                _scopedTransactions = new Stack<TransactionScopeProxy>();
            }
            _scopedTransactions.Push(transaction);
        }

        private ITransaction BeginNewTransaction()
        {
            var transaction = new Neo4jTransactionProxy(_client, GenerateTransaction(), true);
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

            var joinedTransaction = new Neo4jTransactionProxy(_client, GenerateTransaction(parentScope.TransactionContext), false);
            PushScopeTransaction(joinedTransaction);
            return joinedTransaction;
        }

        private ITransaction BeginSupressTransaction()
        {
            var suppressTransaction = new SuppressTransactionProxy(_client);
            PushScopeTransaction(suppressTransaction);
            return suppressTransaction;
        }

        public void EndTransaction()
        {
            TransactionScopeProxy currentTransaction = null;
            try
            {
                currentTransaction = _scopedTransactions == null ? null : _scopedTransactions.Pop();
            }
            catch (InvalidOperationException)
            {
            }
            if (currentTransaction != null)
            {
                currentTransaction.Dispose();
            }
        }

        /// <summary>
        /// Registers to ambient System.Transactions.TransactionContext if needed
        /// </summary>
        public void RegisterToTransactionIfNeeded()
        {
            if (_promotable == null)
            {
                // no need to register as we don't support transactions
                return;
            }
            _promotable.EnlistIfNecessary();
        }

        public Task<HttpResponseMessage> EnqueueCypherRequest(string commandDescription, IGraphClient client, CypherQuery query)
        {
            var policy = new CypherTransactionExecutionPolicy(client);
            // we try to get the current dtc transaction. If we are in a System.Transactions transaction and it has
            // been "promoted" to be handled by DTC then transactionObject will be null, but it doesn't matter as
            // we don't care about updating the object.
            var txContext = GetContext();

            // the main difference with a normal Request.With() call is that the request is associated with the
            // TX context.
            return txContext.EnqueueTask(commandDescription, client, policy, query);
        }

        public void Dispose()
        {
            _scopedTransactions = null;
            Thread.EndThreadAffinity();
        }
    }
}