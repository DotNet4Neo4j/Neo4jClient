using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturned<out TResult> : ICypherFluentQuery
    {
        ICypherFluentQueryReturned<TResult> Limit(int? limit);
        ICypherFluentQueryReturned<TResult> Skip(int? skip);
        ICypherFluentQueryReturned<TResult> OrderBy(params string[] properties);
        ICypherFluentQueryReturned<TResult> OrderByDescending(params string[] properties);

        /// <summary>
        /// Return custom projections i.e. Multiple columns in the result.
        /// </summary>
        IEnumerable<TResult> Results { get; }

        /// <summary>
        /// Return either Nodes, Or Relationships, Paths i.e. Only one column in the result.
        /// </summary>
        IEnumerable<TResult> ResultSet { get; }
    }
}
