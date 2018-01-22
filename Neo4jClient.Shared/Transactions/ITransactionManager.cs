using System;
using System.Net.Http;
using System.Threading.Tasks;
using Neo4jClient.Cypher;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Interface that handles all the queries related to transactions that could be needed in a ITransactionalGraphClient
    /// <see cref="Neo4jClient.Transactions.TransactionManager" /> for implementation.
    /// </summary>
    public interface ITransactionManager<T> : IDisposable
    {
        bool InTransaction { get; }
        ITransaction CurrentNonDtcTransaction { get; }
        ITransaction CurrentDtcTransaction { get; }
        ITransaction BeginTransaction(TransactionScopeOption option);
        void EndTransaction();
        void RegisterToTransactionIfNeeded();
        Task<T> EnqueueCypherRequest(string commandDescription, IGraphClient graphClient, CypherQuery query);
    }
}
