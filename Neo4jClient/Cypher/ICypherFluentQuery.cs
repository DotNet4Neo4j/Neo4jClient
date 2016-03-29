using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Neo4jClient.Cypher
{
    public partial interface ICypherFluentQuery
    {
        CypherQuery Query { get; }
        void ExecuteWithoutResults();
        Task ExecuteWithoutResultsAsync();

        ICypherFluentQuery WithParam(string key, object value);
        ICypherFluentQuery WithParams(IDictionary<string,object> parameters);
        ICypherFluentQuery WithParams(object parameters);

        ICypherFluentQuery ParserVersion(string version);
        ICypherFluentQuery ParserVersion(Version version);
        ICypherFluentQuery ParserVersion(int major, int minor);

        ICypherFluentQuery MaxExecutionTime(int milliseconds);

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
        ICypherFluentQuery Start(object startBits);
        ICypherFluentQuery Start(IDictionary<string, object> startBits);
        [Obsolete("Use Start(new { identity = startText }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
        ICypherFluentQuery Start(string identity, string startText);
        [Obsolete("Use Start(new { foo = nodeRef1, bar = All.Nodes }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
        ICypherFluentQuery Start(params ICypherStartBit[] startBits);
        ICypherFluentQuery Start(string identity, params NodeReference[] nodeReferences);
        ICypherFluentQuery Start(string identity, params RelationshipReference[] relationshipReferences);
        [Obsolete("Use Start(new { foo = Node.ByIndexLookup(�) }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
        ICypherFluentQuery StartWithNodeIndexLookup(string identity, string indexName, string key, object value);
        [Obsolete("Use Start(new { foo = Node.ByIndexQuery(�) }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
        ICypherFluentQuery StartWithNodeIndexLookup(string identity, string indexName, string parameterText);
        ICypherFluentQuery Match(params string[] matchText);
        ICypherFluentQuery UsingIndex(string index);
        ICypherFluentQuery OptionalMatch(string pattern);
        ICypherFluentQuery Merge(string mergeText);
        ICypherFluentQuery OnCreate();
        ICypherFluentQuery OnMatch();
        ICypherFluentQuery CreateUnique(string createUniqueText);
        ICypherFluentQuery CreateUniqueConstraint(string identity, string property);
        ICypherFluentQuery DropUniqueConstraint(string identity, string property);
        ICypherFluentQuery Create(string createText);
        [Obsolete("Use Create(string) with explicitly named params instead. For example, instead of Create(\"(c:Customer {0})\", customer), use Create(\"(c:Customer {customer})\").WithParams(new { customer }).")]
        ICypherFluentQuery Create(string createText, params object[] objects);
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
        /// <returns>An <see cref="ICypherFluentQuery"/> instance to continue the query with.</returns>
        ICypherFluentQuery LoadCsv(Uri fileUri, string identifier);
        ICypherFluentQuery Unwind(string collection, string columnName);
        ICypherFluentQuery Unwind(IEnumerable collection, string identity);
        ICypherFluentQuery Union();
        ICypherFluentQuery UnionAll();
        ICypherFluentQuery Limit(int? limit);
        ICypherFluentQuery Skip(int? skip);
        IOrderedCypherFluentQuery OrderBy(params string[] properties);
        IOrderedCypherFluentQuery OrderByDescending(params string[] properties);
        ICypherFluentQuery<TResult> Return<TResult>(string identity);
        [Obsolete("This was an internal that never should have been exposed. If you want to create a projection, you should be using the lambda overload instead. See the 'Using Functions in Return Clauses' and 'Using Custom Text in Return Clauses' sections of https://bitbucket.org/Readify/neo4jclient/wiki/cypher for details of how to do this.", true)]
        ICypherFluentQuery<TResult> Return<TResult>(string identity, CypherResultMode resultMode);
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

        /// <summary>
        /// Used to control deserialization when building libraries on top of Neo4jClient
        /// </summary>
        /// <typeparam name="TResult">The type to be returned</typeparam>
        /// <param name="expression">The return expression</param>
        /// <returns></returns>
        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(ReturnExpression expression);
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
    }
}
