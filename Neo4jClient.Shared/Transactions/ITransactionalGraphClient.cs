using System;
using System.Collections.Generic;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Describes the options for a transaction scope when calling ITransactionalGraphClient.BeginTransaction
    /// </summary>
    public enum TransactionScopeOption
    {
        Join,
        RequiresNew,
        Suppress
    }

    /// <summary>
    /// Expands the capabilities of a <c>IGraphClient</c> interface to support a transactional model 
    /// for Neo4j HTTP Cypher endpoint.
    /// </summary>
    public interface ITransactionalGraphClient : IGraphClient
    {
        /// <summary>
        /// Scopes the next cypher queries within a transaction, or joins an existing one.
        /// </summary>
        /// <remarks>
        /// This method should only be used when executing multiple Cypher queries
        /// in multiple HTTP requests. Neo4j already encapsulates a single Cypher request within its
        /// own transaction.
        /// 
        /// The transaction object created is thread static, that is, that the following queries will only be within
        /// a transaction for the current thread.
        /// </remarks>
        ITransaction BeginTransaction();

        /// <summary>
        /// Scopes the next cypher queries within a transaction, or joins an existing one.
        /// </summary>
        /// <remarks>
        /// This method should only be used when executing multiple Cypher queries
        /// in multiple HTTP requests. Neo4j already encapsulates a single Cypher request within its
        /// own transaction.
        /// 
        /// The transaction object created is thread static, that is, that the following queries will only be within
        /// a transaction for the current thread.
        /// </remarks>
        /// <param name="bookmark">The bookmark to use for this transaction.</param>
        ITransaction BeginTransaction(string bookmark);

        /// <summary>
        /// Scopes the next cypher queries within a transaction, or joins an existing one.
        /// </summary>
        /// <remarks>
        /// This method should only be used when executing multiple Cypher queries
        /// in multiple HTTP requests. Neo4j already encapsulates a single Cypher request within its
        /// own transaction.
        /// 
        /// The transaction object created is thread static, that is, that the following queries will only be within
        /// a transaction for the current thread.
        /// </remarks>
        /// <param name="bookmarks">The bookmarks to use for this transaction.</param>
        ITransaction BeginTransaction(IEnumerable<string> bookmarks);

        /// <summary>
        /// Scopes the next cypher queries within a transaction (or suppress it), according to a given scope option.
        /// </summary>
        /// <remarks>
        /// This method should be used when executing multiple Cypher queries in multiple HTTP requests,
        /// or when the thread is already under a transaction and the programmer wishes to temporarily suppress it.
        /// 
        /// It should not be called to execute a single Cypher query as it will only add latency to the process. Neo4j already encapsulates
        /// a single Cypher request within its own transaction.
        /// 
        /// Be aware that joining a nested transaction must be done before the parent scope completes either by committing or rolling back,
        /// otherwise it will throw an InvalidOperationException.
        /// 
        /// The transaction object created is thread static, that is, that the following queries will only be within
        /// a transaction for the current thread.
        /// </remarks>
        /// <param name="scopeOption">
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Join</term>
        ///         <description>Creates a new transaction, or joins an existing one. This the default value.
        ///         The transaction commits until all the scope that have a reference to it commit. However, the transaction rolls back 
        ///         on the first call to Rollback().</description>
        ///     </item>
        ///     <item>
        ///         <term>RequiresNew</term>
        ///         <description>The method will generate a new transaction. It is important to notice that this new transaction is not
        /// related to an existent parent transaction scope. Committing or rolling back either one has no effect on the other.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>Suppress</term>
        ///         <description>The statements that are executed under this scope, will not be executed under the transaction.
        ///         Committing or rolling back generates an <c>InvalidOperationException</c>. Creating a new transaction scope with Join under
        ///         a suppressed one, will be the same as RequiresNew.</description>
        ///     </item>
        /// </list>
        /// </param>
        ITransaction BeginTransaction(TransactionScopeOption scopeOption);

        /// <summary>
        /// Scopes the next cypher queries within a transaction (or suppress it), according to a given scope option.
        /// </summary>
        /// <remarks>
        /// This method should be used when executing multiple Cypher queries in multiple HTTP requests,
        /// or when the thread is already under a transaction and the programmer wishes to temporarily suppress it.
        /// 
        /// It should not be called to execute a single Cypher query as it will only add latency to the process. Neo4j already encapsulates
        /// a single Cypher request within its own transaction.
        /// 
        /// Be aware that joining a nested transaction must be done before the parent scope completes either by committing or rolling back,
        /// otherwise it will throw an InvalidOperationException.
        /// 
        /// The transaction object created is thread static, that is, that the following queries will only be within
        /// a transaction for the current thread.
        /// </remarks>
        /// <param name="scopeOption">
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Join</term>
        ///         <description>Creates a new transaction, or joins an existing one. This the default value.
        ///         The transaction commits until all the scope that have a reference to it commit. However, the transaction rolls back 
        ///         on the first call to Rollback().</description>
        ///     </item>
        ///     <item>
        ///         <term>RequiresNew</term>
        ///         <description>The method will generate a new transaction. It is important to notice that this new transaction is not
        /// related to an existent parent transaction scope. Committing or rolling back either one has no effect on the other.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>Suppress</term>
        ///         <description>The statements that are executed under this scope, will not be executed under the transaction.
        ///         Committing or rolling back generates an <c>InvalidOperationException</c>. Creating a new transaction scope with Join under
        ///         a suppressed one, will be the same as RequiresNew.</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="bookmark">The bookmark to use for this transaction.</param>
        ITransaction BeginTransaction(TransactionScopeOption scopeOption, string bookmark);

        /// <summary>
        /// Scopes the next cypher queries within a transaction (or suppress it), according to a given scope option.
        /// </summary>
        /// <remarks>
        /// This method should be used when executing multiple Cypher queries in multiple HTTP requests,
        /// or when the thread is already under a transaction and the programmer wishes to temporarily suppress it.
        /// 
        /// It should not be called to execute a single Cypher query as it will only add latency to the process. Neo4j already encapsulates
        /// a single Cypher request within its own transaction.
        /// 
        /// Be aware that joining a nested transaction must be done before the parent scope completes either by committing or rolling back,
        /// otherwise it will throw an InvalidOperationException.
        /// 
        /// The transaction object created is thread static, that is, that the following queries will only be within
        /// a transaction for the current thread.
        /// </remarks>
        /// <param name="scopeOption">
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Join</term>
        ///         <description>Creates a new transaction, or joins an existing one. This the default value.
        ///         The transaction commits until all the scope that have a reference to it commit. However, the transaction rolls back 
        ///         on the first call to Rollback().</description>
        ///     </item>
        ///     <item>
        ///         <term>RequiresNew</term>
        ///         <description>The method will generate a new transaction. It is important to notice that this new transaction is not
        /// related to an existent parent transaction scope. Committing or rolling back either one has no effect on the other.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>Suppress</term>
        ///         <description>The statements that are executed under this scope, will not be executed under the transaction.
        ///         Committing or rolling back generates an <c>InvalidOperationException</c>. Creating a new transaction scope with Join under
        ///         a suppressed one, will be the same as RequiresNew.</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="bookmarks">The bookmarks to use for this transaction.</param>
        ITransaction BeginTransaction(TransactionScopeOption scopeOption, IEnumerable<string> bookmark);

        /// <summary>
        /// The current transaction object.
        /// </summary>
        /// <remarks>
        /// This object represents our current transactional scope. If it is not null, it doesn't that the code is executing under
        /// a transaction. This latter behavior can occurred when BeginTransaction() is called with TransactionScopeOption.Suppress
        /// </remarks>
        ITransaction Transaction { get; }

        /// <summary>
        /// Determines if the code will be executed under a transaction.
        /// </summary>
        bool InTransaction { get; }

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