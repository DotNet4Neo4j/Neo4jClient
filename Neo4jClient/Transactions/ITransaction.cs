using System;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Represents a Neo4j transaction shared between multiple HTTP requests
    /// </summary>
    /// <remarks>
    /// Neo4j server prevents abandoned transactions from clogging server resources
    /// by rolling back those that do not have requests in the configured timeout period.
    /// </remarks>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Commits our open transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rollbacks any changes made by our open transaction
        /// </summary>
        void Rollback();

        /// <summary>
        /// Prevents the transaction from being claimed as an orphaned transaction.
        /// </summary>
        void KeepAlive();
    }
}
