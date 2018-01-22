using System;
using System.Collections.Generic;
using System.Transactions;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// This class manages the System.Transactions protocol in order to support TransactionScope bindings
    /// </summary>
    internal class TransactionPromotableSinglePhaseNotification : IPromotableSinglePhaseNotification
    {
        private Neo4jRestTransaction transaction;
        private readonly ITransactionalGraphClient client;
        private static ITransactionResourceManager resourceManager;
        private readonly ISet<Transaction> enlistedInTransactions = new HashSet<Transaction>();
        private int transactionId;

        public TransactionPromotableSinglePhaseNotification(ITransactionalGraphClient client)
        {
            this.client = client;
        }

        public void EnlistIfNecessary()
        {
            if (!enlistedInTransactions.Contains(Transaction.Current))
            {
                Enlist(Transaction.Current);
            }
        }

        private void Enlist(Transaction transaction)
        {
            if (transaction == null)
            {
                // no enlistment as we are not in a TransactionScope
                return;
            }

            // try to enlist as a PSPE
            if (!transaction.EnlistPromotableSinglePhase(this))
            {
                // our enlistmente fail so we need to enlist ourselves as durable.

                // we create a transaction directly instead of using BeginTransaction that GraphClient
                // doesn't store it in its stack of scopes.
                 var localTransaction = new Neo4jRestTransaction(client);
                localTransaction.ForceKeepAlive();
                transactionId = localTransaction.Id;
                var resourceManager = GetResourceManager();
                var propagationToken = TransactionInterop.GetTransmitterPropagationToken(transaction);
                var transactionExecutionEnvironment = new TransactionExecutionEnvironment(client.ExecutionConfiguration)
                {
                    TransactionId =  localTransaction.Id,
                    TransactionBaseEndpoint = client.TransactionEndpoint
                };
                resourceManager.Enlist(transactionExecutionEnvironment, propagationToken);
                localTransaction.Cancel();
            }

            enlistedInTransactions.Add(transaction);
        }

        public byte[] Promote()
        {
            // we have been promoted to MSTDC, so we have to clean the local resources
            if (transaction == null)
            {
                transaction = new Neo4jRestTransaction(client);
            }

            // do a keep alive in case the promotion takes too long or in case we don't have an ID
            transaction.ForceKeepAlive();
            transactionId = transaction.Id;
            transaction.Cancel();
            transaction = null;

            if (transactionId == 0)
            {
                throw new InvalidOperationException("For some reason we don't have a TransactionContext ID");
            }

            var resourceManager = GetResourceManager();
            return resourceManager.Promote(new TransactionExecutionEnvironment(client.ExecutionConfiguration)
            {
                TransactionId = transactionId,
                TransactionBaseEndpoint = client.TransactionEndpoint
            });
        }

        public void Initialize()
        {
            // enlistment has completed successfully.
            // For now we can use local transactions
            // we create it directly instead of using BeginTransaction that GraphClient
            // doesn't store it in its stack of scopes.
            transaction = new Neo4jRestTransaction(client);
        }

        public Neo4jRestTransaction AmbientTransaction
        {
            get
            {
                // If _transaction is null, then our PSPE enlistment failed or we got promoted.
                // If we got promoted then we can reconstruct it because we have the id and the client,
                // but only if we have an ID, if we don't have an ID that means we haven't executed a single query
                if (transaction == null && transactionId > 0)
                {
                    return Neo4jRestTransaction.FromIdAndClient(transactionId, client);
                }

                return transaction;
            }
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            // we receive a commit message
            // if we own a local transaction, then commit that transaction
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
                singlePhaseEnlistment.Committed();
            }
            else if (transactionId > 0)
            {
                GetResourceManager().CommitTransaction(transactionId);
                singlePhaseEnlistment.Committed();
            }

            enlistedInTransactions.Remove(Transaction.Current);
        }

        public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            // we receive a commit message
            // if we own a local transaction, then commit that transaction
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }
            else if (transactionId > 0)
            {
                GetResourceManager().RollbackTransaction(transactionId);
            }
            singlePhaseEnlistment.Aborted();

            enlistedInTransactions.Remove(Transaction.Current);
        }

        // the following was adapted from Npgsql sources:
        private static System.Runtime.Remoting.Lifetime.ClientSponsor sponsor;
        private static ITransactionResourceManager GetResourceManager()
        {
            if (resourceManager == null)
            {
                sponsor = new System.Runtime.Remoting.Lifetime.ClientSponsor();
                AppDomain rmDomain = AppDomain.CreateDomain("Neo4jTransactionResourceManager", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
                resourceManager = (ITransactionResourceManager) rmDomain.CreateInstanceAndUnwrap(
                    typeof(Neo4jTransactionResourceManager).Assembly.FullName,
                    typeof(Neo4jTransactionResourceManager).FullName);
                sponsor.Register((MarshalByRefObject)resourceManager);
            }
            return resourceManager;
        }
    }
}
