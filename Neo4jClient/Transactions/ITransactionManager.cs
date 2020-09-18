using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4jClient.Cypher;

namespace Neo4jClient.Transactions
{
    using Neo4j.Driver;

    /// <summary>
    /// Interface that handles all the queries related to transactions that could be needed in a ITransactionalGraphClient
    /// <see cref="Neo4jClient.Transactions.TransactionManager" /> for implementation.
    /// </summary>
    public interface ITransactionManager<T> : IDisposable
    {
        /// <summary>
        /// Is this in a transaction at the moment?
        /// </summary>
        bool InTransaction { get; }
        
        /// <summary>Begins a transaction.</summary>
        /// <param name="option">How should the transaction scope be created.
        /// <see cref="Neo4jClient.Transactions.ITransactionalGraphClient.BeginTransaction(Neo4jClient.Transactions.TransactionScopeOption)" />
        ///  for more information.</param>
        /// <param name="bookmarks">Bookmarks for use with this transaction.</param>
        /// <param name="database">The database to execute the transaction against.</param>
        /// <returns>An <see cref="ITransaction"/> object representing the transaction.</returns>
        ITransaction BeginTransaction(TransactionScopeOption option, IEnumerable<string> bookmarks, string database);
        void EndTransaction();
        ITransaction CurrentTransaction { get; }
        Bookmark LastBookmark { get; }
        Task<T> EnqueueCypherRequest(string commandDescription, IGraphClient graphClient, CypherQuery query);
    }
}
