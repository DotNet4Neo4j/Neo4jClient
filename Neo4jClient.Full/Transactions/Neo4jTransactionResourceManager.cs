using System;
using System.Collections.Generic;
using System.Transactions;

namespace Neo4jClient.Transactions
{
    internal class BoltTransactionSinglePhaseNotification : ISinglePhaseNotification
    {
        private readonly ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment;

        public BoltTransactionSinglePhaseNotification(ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment)
        {
            this.transactionExecutionEnvironment = transactionExecutionEnvironment;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
//            BoltNeo4jTransaction.DoKeepAlive(transactionExecutionEnvironment);
            preparingEnlistment.Done();
        }

        public void Commit(Enlistment enlistment)
        {
            try
            {
                BoltNeo4jTransaction.DoCommit(transactionExecutionEnvironment);
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
              BoltNeo4jTransaction.DoRollback(transactionExecutionEnvironment);
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
                BoltNeo4jTransaction.DoCommit(transactionExecutionEnvironment);
                singlePhaseEnlistment.Committed();
            }
            finally
            {
                singlePhaseEnlistment.Aborted();
            }
        }

        public void Enlist(Transaction tx)
        {
            tx.EnlistDurable(transactionExecutionEnvironment.ResourceManagerId, this, EnlistmentOptions.None);
        }
    }


    /// <summary>
    /// When <c>TransactionPromotableSinglePhaseNotification</c> fails to register as PSPE, then this class will
    /// be registered, and all the necessary work will be done in here
    /// </summary>
    internal class Neo4jTransationSinglePhaseNotification : ISinglePhaseNotification
    {
        private readonly ITransactionExecutionEnvironment transactionExecutionEnvironment;

        public Neo4jTransationSinglePhaseNotification(ITransactionExecutionEnvironment transactionExecutionEnvironment)
        {
            this.transactionExecutionEnvironment = transactionExecutionEnvironment;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Neo4jRestTransaction.DoKeepAlive(transactionExecutionEnvironment);
            preparingEnlistment.Done();
        }

        public void Commit(Enlistment enlistment)
        {
            try
            {
                Neo4jRestTransaction.DoCommit(transactionExecutionEnvironment);
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
                Neo4jRestTransaction.DoRollback(transactionExecutionEnvironment);
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
                Neo4jRestTransaction.DoCommit(transactionExecutionEnvironment);
                singlePhaseEnlistment.Committed();
            }
            finally
            {
                singlePhaseEnlistment.Aborted();
            }
        }

        public void Enlist(Transaction tx)
        {
            tx.EnlistDurable(transactionExecutionEnvironment.ResourceManagerId, this, EnlistmentOptions.None);
        }
    }

    internal class BoltTransactionResourceManager : MarshalByRefObject, ITransactionResourceManagerBolt
    {
        private readonly IDictionary<Guid, CommittableTransaction> transactions = new Dictionary<Guid, CommittableTransaction>();

        public void RollbackTransaction(Guid transactionId)
        {
            CommittableTransaction tx;
            if (transactions.TryGetValue(transactionId, out tx))
            {
                transactions.Remove(transactionId);
                tx.Rollback();
            }
        }

        public void CommitTransaction(Guid transactionId)
        {
            CommittableTransaction tx;
            if (transactions.TryGetValue(transactionId, out tx))
            {
                tx.Commit();
                tx.Dispose();
                transactions.Remove(transactionId);
            }
        }

        public void Enlist(ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment, byte[] transactionToken)
        {
            var tx = TransactionInterop.GetTransactionFromTransmitterPropagationToken(transactionToken);
            new BoltTransactionSinglePhaseNotification(transactionExecutionEnvironment).Enlist(tx);
        }

        public byte[] Promote(ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment)
        {
            var promotedTx = new CommittableTransaction();
            var neo4jTransactionHandler = new BoltTransactionSinglePhaseNotification(transactionExecutionEnvironment);
            var token = TransactionInterop.GetTransmitterPropagationToken(promotedTx);
            transactions[transactionExecutionEnvironment.TransactionId] = promotedTx;
            neo4jTransactionHandler.Enlist(promotedTx);

            return token;
        }
    }

    internal class Neo4jTransactionResourceManager : MarshalByRefObject, ITransactionResourceManager
    {
        private readonly IDictionary<int, CommittableTransaction> transactions = new Dictionary<int, CommittableTransaction>();
        
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
            transactions[transactionExecutionEnvironment.TransactionId] = promotedTx;
            neo4jTransactionHandler.Enlist(promotedTx);

            return token;
        }

        public void CommitTransaction(int transactionId)
        {
            CommittableTransaction tx;
            if (transactions.TryGetValue(transactionId, out tx))
            {
                tx.Commit();
                transactions.Remove(transactionId);
            }
        }

        public void RollbackTransaction(int transactionId)
        {
            CommittableTransaction tx;
            if (transactions.TryGetValue(transactionId, out tx))
            {
                transactions.Remove(transactionId);
                tx.Rollback();
            }
        }

    }
}
