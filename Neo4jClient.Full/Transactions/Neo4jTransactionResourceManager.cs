using System;
using System.Collections.Generic;
using System.Transactions;

namespace Neo4jClient.Transactions
{

    /// <summary>
    /// When <c>TransactionPromotableSinglePhaseNotification</c> fails to register as PSPE, then this class will
    /// be registered, and all the necessary work will be done in here
    /// </summary>
    internal class Neo4jTransationSinglePhaseNotification : ISinglePhaseNotification
    {
        private ITransactionExecutionEnvironment _transactionExecutionEnvironment;

        public Neo4jTransationSinglePhaseNotification(ITransactionExecutionEnvironment transactionExecutionEnvironment)
        {
            _transactionExecutionEnvironment = transactionExecutionEnvironment;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Neo4jTransaction.DoKeepAlive(_transactionExecutionEnvironment);
            preparingEnlistment.Done();
        }

        public void Commit(Enlistment enlistment)
        {
            try
            {
                Neo4jTransaction.DoCommit(_transactionExecutionEnvironment);
            }
            finally
            {
                // always have to call Done() or we clog the resources
                enlistment.Done();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            try
            {
                Neo4jTransaction.DoRollback(_transactionExecutionEnvironment);
            }
            finally
            {
                // always have to call Done() or we clog the resources
                enlistment.Done();
            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            try
            {
                Neo4jTransaction.DoCommit(_transactionExecutionEnvironment);
                singlePhaseEnlistment.Committed();
            }
            finally
            {
                singlePhaseEnlistment.Aborted();
            }
        }

        public void Enlist(Transaction tx)
        {
            tx.EnlistDurable(_transactionExecutionEnvironment.ResourceManagerId, this, EnlistmentOptions.None);
        }
    }

    internal class Neo4jTransactionResourceManager : MarshalByRefObject, ITransactionResourceManager
    {
        private readonly IDictionary<int, CommittableTransaction> _transactions = new Dictionary<int, CommittableTransaction>();

        public void Enlist(ITransactionExecutionEnvironment transactionExecutionEnvironment, byte[] transactionToken)
        {
            var tx = TransactionInterop.GetTransactionFromTransmitterPropagationToken(transactionToken);
            new Neo4jTransationSinglePhaseNotification(transactionExecutionEnvironment).Enlist(tx);
        }

        public byte[] Promote(ITransactionExecutionEnvironment transactionExecutionEnvironment)
        {
            var promotedTx = new CommittableTransaction();
            var neo4jTransactionHandler = new Neo4jTransationSinglePhaseNotification(transactionExecutionEnvironment);
            var token = TransactionInterop.GetTransmitterPropagationToken(promotedTx);
            _transactions[transactionExecutionEnvironment.TransactionId] = promotedTx;
            neo4jTransactionHandler.Enlist(promotedTx);

            return token;
        }

        public void CommitTransaction(int transactionId)
        {
            CommittableTransaction tx;
            if (_transactions.TryGetValue(transactionId, out tx))
            {
                tx.Commit();
                _transactions.Remove(transactionId);
            }
        }

        public void RollbackTransaction(int transactionId)
        {
            CommittableTransaction tx;
            if (_transactions.TryGetValue(transactionId, out tx))
            {
                _transactions.Remove(transactionId);
                tx.Rollback();
            }
        }
    }
}
