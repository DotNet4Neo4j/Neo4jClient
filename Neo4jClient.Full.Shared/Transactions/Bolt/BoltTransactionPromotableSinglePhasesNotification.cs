using System;
using System.Collections.Generic;
using System.Transactions;

namespace Neo4jClient.Transactions
{
    internal class BoltTransactionPromotableSinglePhasesNotification : IPromotableSinglePhaseNotification
    {
        private readonly ITransactionalGraphClient client;
        private readonly ISet<Transaction> enlistedInTransactions = new HashSet<Transaction>();
        private BoltNeo4jTransaction transaction;

        public BoltTransactionPromotableSinglePhasesNotification(ITransactionalGraphClient client)
        {
            this.client = client;
        }
        #region Implementation of ITransactionPromoter

        /// <inheritdoc />
        public byte[] Promote()
        {
            // we have been promoted to MSTDC, so we have to clean the local resources
            if (transaction == null)
            {
                transaction = new BoltNeo4jTransaction(((BoltGraphClient)client).Driver );
            }
            
            transaction.Cancel();
            transactionId = transaction.Id;
            var driverTx = transaction.DriverTransaction;
            var session = transaction.Session;
            transaction = null;
            
            return ResourceManager.Promote(new BoltTransactionExecutionEnvironment(client.ExecutionConfiguration)
            {
                TransactionId = transactionId,
                Session = session,
                DriverTransaction = driverTx
            });
        }

        #endregion

        #region Implementation of IPromotableSinglePhaseNotification

        /// <inheritdoc />
        public void Initialize()
        {
            transaction = new BoltNeo4jTransaction(((BoltGraphClient)client).Driver);
        }

        /// <inheritdoc />
        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            // we receive a commit message
            // if we own a local transaction, then commit that transaction
            if (transaction != null)
            {
                transaction.Commit();
                transaction.Dispose();
                transaction = null;
                singlePhaseEnlistment.Committed();
            }
            else if(transactionId != Guid.Empty)
            {
                ResourceManager.CommitTransaction(transactionId);
                singlePhaseEnlistment.Committed();
            }

            enlistedInTransactions.Remove(Transaction.Current);
        }

        /// <inheritdoc />
        public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction.Dispose();
                transaction = null;
            }
            else if (transactionId != Guid.Empty)
            {
                ResourceManager.RollbackTransaction(transactionId);
            }
            singlePhaseEnlistment.Aborted();

            enlistedInTransactions.Remove(Transaction.Current);
        }

        #endregion

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
                var localTransaction = new BoltNeo4jTransaction(((BoltGraphClient)client).Driver);

               
                var propagationToken = TransactionInterop.GetTransmitterPropagationToken(transaction);
                var transactionExecutionEnvironment = new BoltTransactionExecutionEnvironment(client.ExecutionConfiguration)
                {
                    Session = this.transaction.Session,
                    DriverTransaction = this.transaction.DriverTransaction,
                    TransactionId = this.transactionId
                    //                    TransactionId = localTransaction.Id,
                    //                    TransactionBaseEndpoint = client.TransactionEndpoint
                };
                ResourceManager.Enlist(transactionExecutionEnvironment, propagationToken);
                localTransaction.Cancel();
            }

            enlistedInTransactions.Add(transaction);
        }

        public BoltNeo4jTransaction AmbientTransaction
        {
            get
            {
//                // If _transaction is null, then our PSPE enlistment failed or we got promoted.
//                // If we got promoted then we can reconstruct it because we have the id and the client,
//                // but only if we have an ID, if we don't have an ID that means we haven't executed a single query
                if (transaction == null && transactionId == Guid.Empty)
                {
                    return BoltNeo4jTransaction.FromIdAndClient(transactionId, ((BoltGraphClient)client).Driver);
                }

                return transaction;
            }
        }
        //Need to set this for bolt transactions as well.
        private Guid transactionId;
        private static ITransactionResourceManagerBolt ResourceManager
        {
            get
            {
                if (transactionResourceManager == null)
                {
                    sponsor = new System.Runtime.Remoting.Lifetime.ClientSponsor();
                    AppDomain rmDomain = AppDomain.CreateDomain(nameof(BoltTransactionResourceManager), AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
                    transactionResourceManager = (ITransactionResourceManagerBolt)rmDomain.CreateInstanceAndUnwrap(
                        typeof(BoltTransactionResourceManager).Assembly.FullName,
                        typeof(BoltTransactionResourceManager).FullName);
                    sponsor.Register((MarshalByRefObject)transactionResourceManager);
                }
                return transactionResourceManager;
            }
        }
        private static ITransactionResourceManagerBolt transactionResourceManager;

       
        // the following was adapted from Npgsql sources (then from TransactionSinglePhaseNotification):
        private static System.Runtime.Remoting.Lifetime.ClientSponsor sponsor;

        
    }
}