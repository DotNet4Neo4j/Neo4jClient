using System;
using System.Collections.Generic;
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
        //TODO: Remove - there is no concept of a DTC/NonDTC transaction anymore
        ITransaction CurrentNonDtcTransaction { get; }
        ITransaction BeginTransaction(TransactionScopeOption option, IEnumerable<string> bookmarks);
        void EndTransaction();
        Task<T> EnqueueCypherRequest(string commandDescription, IGraphClient graphClient, CypherQuery query);
    }
}
