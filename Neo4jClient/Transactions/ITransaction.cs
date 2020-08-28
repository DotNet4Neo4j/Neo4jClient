using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Neo4jClient.Transactions
{
    using Neo4j.Driver;

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
        /// In 4 + databases, which database this transaction is against.
        /// </summary>
        string Database { get;  }

        /// <summary>
        /// Commits our open transaction.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Rollbacks any changes made by our open transaction
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Prevents the transaction from being claimed as an orphaned transaction.
        /// </summary>
        Task KeepAliveAsync();

        /// <summary>
        /// Returns true if the transaction is still open, that is, if the programmer has not called
        /// Commit() or Rollback().
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Custom header collection This will be the same for the entire transaction.
        /// So the commit will use the same custom header(s) as the cypher custom header
        /// </summary>
        NameValueCollection CustomHeaders { get; set; }

        /// <summary>
        /// The last bookmark provided by the database to allow for causal consistency.
        /// </summary>
        Bookmark LastBookmark { get; }
    }
}
