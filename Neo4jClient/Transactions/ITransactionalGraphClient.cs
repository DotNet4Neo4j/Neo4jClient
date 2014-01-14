using System;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Expands the capabilities of a <c>IGraphClient</c> interface to support a transactional model 
    /// for Neo4j HTTP Cypher endpoint.
    /// </summary>
    public interface ITransactionalGraphClient : IGraphClient
    {
        /// <summary>
        /// Scopes the next cypher queries within a transaction.
        /// </summary>
        /// <remarks>
        /// This method should only be used when multiple executing multiple Cypher queries
        /// in multiple HTTP requests. Neo4j already encapsulates a single Cypher request within its
        /// own transaction.
        /// </remarks>
        ITransaction BeginTransaction();

        /// <summary>
        /// The current transaction object.
        /// </summary>
        ITransaction Transaction { get; }

        /// <summary>
        /// Closes the scope of a transaction. The <c>ITransaction</c> will behave as if it was being disposed.
        /// </summary>
        void EndTransaction();

        /// <summary>
        /// The Neo4j transaction initial transaction endpoint
        /// </summary>
        Uri TransactionEndpoint { get; }
    }
}