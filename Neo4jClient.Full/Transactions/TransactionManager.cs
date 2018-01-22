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
    /// <summary>
    /// Handles all the queries related to transactions that could be needed in a ITransactionalGraphClient
    /// </summary>
    internal class TransactionManager : ITransactionManager<HttpResponseMessage>
    {
        // holds the transaction objects per thread
        private static AsyncLocal<Stack<TransactionScopeProxy>> scopedTransactions;
        // holds the transaction contexts for transactions from the System.Transactions framework
        private readonly IDictionary<string, TransactionContext> dtcContexts; 
        private readonly TransactionPromotableSinglePhaseNotification promotable;
        private readonly ITransactionalGraphClient client;

        internal AsyncLocal<Stack<TransactionScopeProxy>>  ScopedTransactions
        {
            get
            {
                if (scopedTransactions?.Value == null)
                    scopedTransactions = CreateScopedTransactions();
                return scopedTransactions;
            }
            set { scopedTransactions = value; }
        }

        private static AsyncLocal<Stack<TransactionScopeProxy>> CreateScopedTransactions()
        {
            return new AsyncLocal<Stack<TransactionScopeProxy>> { Value = new Stack<TransactionScopeProxy>() };
        }

        public TransactionManager(ITransactionalGraphClient client)
        {
            this.client = client;
            // specifies that we are about to use variables that depend on OS threads
            Thread.BeginThreadAffinity();
            scopedTransactions = CreateScopedTransactions();

            // this object enables the interacion with System.Transactions and MSDTC, at first by
            // letting us manage the transaction objects ourselves, and if we require to be promoted to MSDTC,
            // then it notifies the library how to do it.
            promotable = new TransactionPromotableSinglePhaseNotification(client);
            dtcContexts = new Dictionary<string, TransactionContext>();
        }

        private TransactionContext GetOrCreateDtcTransactionContext(NameValueCollection customHeaders = null)
        {
            // we need to lock as we could get other async requests to the same transaction
            var txId = Transaction.Current.TransactionInformation.LocalIdentifier;
            lock (dtcContexts)
            {
                TransactionContext txContext;
                if (dtcContexts.TryGetValue(txId, out txContext))
                {
                    return txContext;
                }

                // associate it with the ambient transaction
                txContext = new TransactionContext(promotable.AmbientTransaction)
                {
                    Transaction = {CustomHeaders = customHeaders},
                    CustomHeaders = customHeaders
                };
                dtcContexts[txId] = txContext;
                
                return txContext;
            }
        }

        private TransactionContext GetContext(NameValueCollection customHeaders = null)
        {
            var nonDtcTransaction = CurrentInternalTransaction;
            if (nonDtcTransaction != null && nonDtcTransaction.Committable)
            {
                return nonDtcTransaction.TransactionContext;
            }

            // if we are not in a native transaction get the context of our ambient transaction
            return GetOrCreateDtcTransactionContext(customHeaders);
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

        public TransactionScopeProxy CurrentInternalTransaction
        {
            get
            {
                if (scopedTransactions?.Value == null || scopedTransactions.Value.Count == 0)
                    return null;
                return scopedTransactions.Value.Peek();
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
        /// <returns></returns>
        public ITransaction BeginTransaction(TransactionScopeOption scopeOption)
        {
            if (scopeOption == TransactionScopeOption.Suppress)
            {
                // TransactionScopeOption.Suppress doesn't fail with older versions of Neo4j
                return BeginSupressTransaction();
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
            return BeginNewTransaction();
        }

        private TransactionContext GenerateTransaction()
        {
            return new TransactionContext(new Neo4jRestTransaction(client));
        }

        private TransactionContext GenerateTransaction(TransactionContext reference)
        {
            return new TransactionContext(reference.Transaction);
        }

        private static void PushScopeTransaction(TransactionScopeProxy transaction)
        {
            if (scopedTransactions?.Value == null)
            {
                scopedTransactions = CreateScopedTransactions();
            }
            scopedTransactions.Value.Push(transaction);
        }

        private ITransaction BeginNewTransaction()
        {
            var transaction = new Neo4jTransactionProxy(client, GenerateTransaction(), true);
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

        private ITransaction BeginSupressTransaction()
        {
            var suppressTransaction = new SuppressTransactionProxy(client);
            PushScopeTransaction(suppressTransaction);
            return suppressTransaction;
        }

        public void EndTransaction()
        {
            if (scopedTransactions?.Value == null || scopedTransactions.Value.Count <= 0)
                return;

            var currentTransaction = scopedTransactions.Value.Pop();
            if (currentTransaction != null)
                currentTransaction.Dispose();
        }

        /// <summary>
        /// Registers to ambient System.Transactions.TransactionContext if needed
        /// </summary>
        public void RegisterToTransactionIfNeeded()
        {
            promotable?.EnlistIfNecessary();
        }

        public Task<HttpResponseMessage> EnqueueCypherRequest(string commandDescription, IGraphClient graphClient, CypherQuery query)
        {
            var policy = new CypherTransactionExecutionPolicy(graphClient);
            // we try to get the current dtc transaction. If we are in a System.Transactions transaction and it has
            // been "promoted" to be handled by DTC then transactionObject will be null, but it doesn't matter as
            // we don't care about updating the object.
            var txContext = GetContext(query.CustomHeaders);
            txContext.CustomHeaders = query.CustomHeaders;
            // the main difference with a normal Request.With() call is that the request is associated with the
            // TX context.
            return txContext.EnqueueTask(commandDescription, graphClient, policy, query);
        }

        public void Dispose()
        {
            scopedTransactions = null;
            Thread.EndThreadAffinity();
        }
    }
}