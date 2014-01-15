using System;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Interface that handles all the queries related to transactions that could be needed in a ITransactionalGraphClient
    /// <see cref="Neo4jClient.Transactions.TransactionManager" /> for implementation.
    /// </summary>
    public interface ITransactionManager : IDisposable
    {
        bool InTransaction { get; }
        ITransaction CurrentNonDtcTransaction { get; }
        ITransaction CurrentDtcTransaction { get; }
        ITransaction BeginTransaction(TransactionScopeOption option);
        void EndTransaction();
        void RegisterToTransactionIfNeeded();
    }
}
