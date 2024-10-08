using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Neo4jClient.Cypher
{
    public partial interface ICypherFluentQuery
    {
        CypherQuery Query { get; }
        Task ExecuteWithoutResultsAsync();

        ICypherFluentQueryAdvanced Advanced { get; }
        /// <summary>
        /// When the result is returned, the <see cref="IGraphClient.OperationCompleted"/> event will contain the <see cref="QueryStats"/> if requested.
        /// </summary>
        ICypherFluentQuery WithQueryStats { get; }
        ICypherFluentQuery WithParam(string key, object value);
        ICypherFluentQuery WithParams(IDictionary<string,object> parameters);
        ICypherFluentQuery WithParams(object parameters);

        ICypherFluentQuery ParserVersion(string version);
        ICypherFluentQuery ParserVersion(Version version);
        ICypherFluentQuery ParserVersion(int major, int minor);

        ICypherFluentQuery MaxExecutionTime(int milliseconds);
        
        // /// <summary>
        // /// Calls 'SHOW' on the database
        // /// </summary>
        // /// <param name="command">The command to execute after SHOW - for example 'DATABASES'</param>
        // /// <returns>An <see cref="ICypherFluentQuery" /> instance to continue the query with.</returns>
        // ICypherFluentQuery Show(string command);

        /// <summary>
        /// Custom headers to add to REST calls to Neo4j server.
        /// Example usage: This can be used to provide extra information to a Neo4j Loadbalancer.
        /// </summary>
        /// <remarks>
        /// This settings is ignored when using <see cref="IRawGraphClient.ExecuteMultipleCypherQueriesInTransaction"/>
        /// Since it could create a race-condition.
        /// </remarks>
        /// <param name="headers">Customheader added via: <see cref="NameValueCollection"/></param>
        /// <returns></returns>
        ICypherFluentQuery CustomHeaders(NameValueCollection headers);

        ICypherFluentQuery Planner(string planner);
        ICypherFluentQuery Planner(CypherPlanner planner);

        ICypherFluentQuery Runtime(string runtime);
        ICypherFluentQuery Runtime(CypherRuntime runtime);
       
        ICypherFluentQuery Match(params string[] matchText);
        ICypherFluentQuery UsingIndex(string index);
        ICypherFluentQuery OptionalMatch(string pattern);
        ICypherFluentQuery Merge(string mergeText);
        ICypherFluentQuery OnCreate();
        ICypherFluentQuery OnMatch();
        
        /// <summary>
        /// [Neo4j 3.0+] Calls a Stored Procedure on the Database.
        /// </summary>
        /// <remarks>This only works on Neo4j 3.0+</remarks>
        /// <param name="storedProcedureText">The Stored Procedure to execute.</param>
        /// <returns>An <see cref="ICypherFluentQuery"/> instance to continue the query with.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="storedProcedureText"/> argument is empty or null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an attempt is made to call this against a server version prior to 3.0.</exception>
        ICypherFluentQuery Call(string storedProcedureText);

        /// <summary>
        /// [Neo4j 4.0+] Calls a SubQuery on the Database.
        /// </summary>
        /// <remarks>This only works on Neo4j 4.0+</remarks>
        /// <param name="storedProcedureText">The Sub Query to execute.</param>
        /// <returns>An <see cref="ICypherFluentQuery"/> instance to continue the query with.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="storedProcedureText"/> argument is empty or null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an attempt is made to call this against a server version prior to 3.0.</exception>
        ICypherFluentQuery Call<T>(Func<ICypherFluentQuery<T>> subQuery);

        /// <summary>
        /// [Neo4j 4.4+] Batches subqueries 
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        ICypherFluentQuery InTransactions(int? rows = null);

        /// <summary>
        /// [Neo4j 3.0+] Yields the values from the response of a <see cref="Call"/> method
        /// </summary>
        /// <remarks>This only works on Neo4j 3.0+</remarks>
        /// <param name="yieldText">The values to yield.</param>
        /// <returns>An <see cref="ICypherFluentQuery"/> instance to continue the query with.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="yieldText"/> argument is empty or null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an attempt is made to call this against a server version prior to 3.0.</exception>
        ICypherFluentQuery Yield(string yieldText);

        ICypherFluentQuery CreateUnique(string createUniqueText);

        /// <summary>
        /// Creates a <c>UNIQUE CONSTRAINT</c> in the database.
        /// </summary>
        /// <remarks>Executes <c>CREATE CONSTRAINT ON (<paramref name="identity">identity</paramref>) ASSERT <paramref name="property">property</paramref> IS UNIQUE</c></remarks>
        /// <param name="identity">The identity to use to create the constraint, you must include the Label, example: <c>"u:User"</c></param>
        /// <param name="property">The property to index, this should use the <paramref name="identity"/> defined, example: <c>"u.id"</c></param>
        /// <returns>An <see cref="ICypherFluentQuery"/> instance to continue the query with.</returns>
        ICypherFluentQuery CreateUniqueConstraint(string identity, string property);


        //ICypherFluentQuery CreateUniqueConstraint(string nameOfConstraint, string label, string identifier, string property);

        ICypherFluentQuery DropUniqueConstraint(string identity, string property);
        ICypherFluentQuery Create(string createText);
        ICypherFluentQuery Delete(string identities);

        ICypherFluentQuery DetachDelete(string identities);
        ICypherFluentQuery Drop(string dropText);
        ICypherFluentQuery Set(string setText);
        ICypherFluentQuery Remove(string removeText);
        ICypherFluentQuery ForEach(string text);

        /// <summary>Load a CSV into Neo4j.</summary>
        /// <remarks>This creates the following: <c>LOAD CSV FROM 'fileUri' AS identifier</c></remarks>
        /// <param name="fileUri">The file URI in the format: <c>c:\\example.csv.</c></param>
        /// <param name="identifier">The identifier to use in the rest of the query.</param>
        /// <param name="withHeaders">Use this custom headers are defined in the CSV file</param>
        /// <param name="fieldTerminator">The terminator to use. If you do not define anything the Neo4j default will be used: ','</param>
        /// <param name="periodicCommit">If this is <c>null</c> no periodic commit will be used, otherwise PERIODIC COMMIT will be added with the value set.</param>
        /// <returns>An <see cref="ICypherFluentQuery"/> instance to continue the query with.</returns>
        ICypherFluentQuery LoadCsv(Uri fileUri, string identifier, bool withHeaders = false, string fieldTerminator = null, int? periodicCommit = null);
        ICypherFluentQuery Unwind(string collection, string columnName);
        ICypherFluentQuery Unwind(IEnumerable collection, string identity);
        ICypherFluentQuery Union();
        ICypherFluentQuery UnionAll();
        ICypherFluentQuery Limit(int? limit);
        ICypherFluentQuery Skip(int? skip);
        IOrderedCypherFluentQuery OrderBy(params string[] properties);
        IOrderedCypherFluentQuery OrderByDescending(params string[] properties);
        ICypherFluentQuery<TResult> Return<TResult>(string identity);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> Return<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);

        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(string identity);
//        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(Expression<Func<ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, ICypherResultItem, TResult>> expression);

        /// <summary>
        /// Set the query to be a Read query.
        /// </summary>
        /// <remarks>This is used by the <see cref="BoltGraphClient"/></remarks>
        /// /// <returns>A <see cref="ICypherFluentQuery"/> instance to continue querying with.</returns>
        ICypherFluentQuery Read { get; }

        /// <summary>
        /// Set the query to be a Write query (which is the default behaviour).
        /// </summary>
        /// <remarks>This is used by the <see cref="BoltGraphClient"/></remarks>
        /// <returns>A <see cref="ICypherFluentQuery"/> instance to continue querying with.</returns>
        ICypherFluentQuery Write { get; }

        /// <summary>
        /// Sets the database instance to use to run the queries against.
        /// </summary>
        /// <param name="databaseName">The name of the database instance.</param>
        /// <returns>An <see cref="ICypherFluentQuery"/> instance to continue querying with.</returns>
        ICypherFluentQuery WithDatabase(string databaseName);

        /// <summary>
        /// Set an identifier for your query. You don't need to set this, it's intended to allow you to link an <see cref="IGraphClient.OperationCompleted"/> event to a specific query.
        /// </summary>
        /// <param name="identifier">An ID you want to use to differentiate the query. If this is <c>null</c> or whitespace - it will be set to <c>null</c>.</param>
        /// <returns>An <see cref="ICypherFluentQuery"/> to query with.</returns>
        ICypherFluentQuery WithIdentifier(string identifier);

        /// <summary>
        /// Perform the query using the given <paramref name="bookmarks"/>.
        /// </summary>
        /// <remarks>This only works for a <strong>Bolt</strong> session.</remarks>
        /// <param name="bookmark">The bookmark to use.</param>
        /// <returns>A <see cref="ICypherFluentQuery"/> instance to continue querying with.</returns>
        ICypherFluentQuery WithBookmark(string bookmark);

        /// <summary>
        /// Perform the query using the given <paramref name="bookmarks"/>.
        /// </summary>
        /// <remarks>This only works for a <strong>Bolt</strong> session.</remarks>
        /// <param name="bookmarks">The bookmarks to use.</param>
        /// <returns>A <see cref="ICypherFluentQuery"/> instance to continue querying with.</returns>
        ICypherFluentQuery WithBookmarks(params string[] bookmarks);

        /// <summary>
        /// The <c>USE</c> clause determines which graph a query, or query part, is executed against.
        /// It is supported for queries and schema commands.
        /// </summary>
        /// <remarks>Neo4j 4.x+</remarks>
        /// <param name="graphName">The name of the graph to run the next commands against.</param>
        /// <returns>A <see cref="ICypherFluentQuery"/> instance to continue querying with.</returns>
        ICypherFluentQuery Use(string graphName);
    }
}
