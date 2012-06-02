using System;
using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturned<out TResult> : ICypherFluentQuery
    {
        ICypherFluentQueryReturned<TResult> Limit(int? limit);
        ICypherFluentQueryReturned<TResult> Skip(int? skip);
        ICypherFluentQueryReturned<TResult> OrderBy(params string[] properties);
        ICypherFluentQueryReturned<TResult> OrderByDescending(params string[] properties);

        IEnumerable<TResult> Results { get; }

        [Obsolete("Use the Results property instead.", true)]
        IEnumerable<TResult> ResultSet { get; }
    }
}
