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
        private Neo4jTransaction _transaction;
        private readonly ITransactionalGraphClient _client;
        private static ITransactionResourceManager _resourceManager;
        private ISet<Transaction> _enlistedInTransactions = new HashSet<Transaction>();
        private int _transactionId;

        public TransactionPromotableSinglePhaseNotification(ITransactionalGraphClient client)
        {
            _client = client;
            //_resourceManager = new Neo4jTransactionResourceManager();
        }

        public void EnlistIfNecessary()
        {
            if (!_enlistedInTransactions.Contains(Transaction.Current))
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
                 var localTransaction = new Neo4jTransaction(_client);
                localTransaction.ForceKeepAlive();
                var resourceManager = GetResourceManager();
                var propagationToken = TransactionInterop.GetTransmitterPropagationToken(transaction);
                var transactionExecutionEnvironment = new TransactionExecutionEnvironment(_client.ExecutionConfiguration)
                {
                    TransactionId =  localTransaction.Id,
                    TransactionBaseEndpoint = _client.TransactionEndpoint
                };
                resourceManager.Enlist(transactionExecutionEnvironment, propagationToken);
                localTransaction.Cancel();
            }

            _enlistedInTransactions.Add(transaction);
        }

        public byte[] Promote()
        {
            // we have been promoted to MSTDC, so we have to clean the local resources
            if (_transaction == null)
            {
                _transaction = new Neo4jTransaction(_client);
            }

            // do a keep alive in case the promotion takes too long or in case we don't have an ID
            _transaction.ForceKeepAlive();
            _transactionId = _transaction.Id;
            _transaction.Cancel();
            _transaction = null;

            if (_transactionId == 0)
            {
                throw new InvalidOperationException("For some reason we don't have a Transaction ID");
            }

            var resourceManager = GetResourceManager();
            return resourceManager.Promote(new TransactionExecutionEnvironment(_client.ExecutionConfiguration)
            {
                TransactionId = _transactionId,
                TransactionBaseEndpoint = _client.TransactionEndpoint
            });
        }

        public void Initialize()
        {
            // enlistment has completed successfully.
            // For now we can use local transactions
            // we create it directly instead of using BeginTransaction that GraphClient
            // doesn't store it in its stack of scopes.
            _transaction = new Neo4jTransaction(_client);
        }

        public Neo4jTransaction AmbientTransaction
        {
            get
            {
                // If _transaction is null, then our PSPE enlistment failed or we got promoted.
                // If we got promoted then we can reconstruct it because we have the id and the client,
                // but only if we have an ID, if we don't have an ID that means we haven't executed a single query
                if (_transaction == null && _transactionId > 0)
                {
                    return Neo4jTransaction.FromIdAndClient(_transactionId, _client);
                }

                return _transaction;
            }
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            // we receive a commit message
            // if we own a local transaction, then commit that transaction
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
                singlePhaseEnlistment.Committed();
            }
            else if (_transactionId > 0)
            {
                GetResourceManager().CommitTransaction(_transactionId);
                singlePhaseEnlistment.Committed();
            }

            _enlistedInTransactions.Remove(Transaction.Current);
        }

        public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            // we receive a commit message
            // if we own a local transaction, then commit that transaction
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
            }
            else if (_transactionId > 0)
            {
                GetResourceManager().RollbackTransaction(_transactionId);
            }
            singlePhaseEnlistment.Aborted();

            _enlistedInTransactions.Remove(Transaction.Current);
        }

        // the following was adapted from Npgsql sources:
        private static System.Runtime.Remoting.Lifetime.ClientSponsor _sponser;
        private static ITransactionResourceManager GetResourceManager()
        {
            if (_resourceManager == null)
            {
                _sponser = new System.Runtime.Remoting.Lifetime.ClientSponsor();
                AppDomain rmDomain = AppDomain.CreateDomain("Neo4jTransactionResourceManager", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
                _resourceManager = (ITransactionResourceManager) rmDomain.CreateInstanceAndUnwrap(
                    typeof(Neo4jTransactionResourceManager).Assembly.FullName,
                    typeof(Neo4jTransactionResourceManager).FullName);
                _sponser.Register((MarshalByRefObject)_resourceManager);
            }
            return _resourceManager;
        }
    }
}
