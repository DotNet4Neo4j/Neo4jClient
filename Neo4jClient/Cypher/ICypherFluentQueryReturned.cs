using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturned<out TResult> : ICypherFluentQuery
    {
        ICypherFluentQueryReturned<TResult> Limit(int? limit);
        ICypherFluentQueryReturned<TResult> OrderBy(params string[] properties);
        ICypherFluentQueryReturned<TResult> OrderByDescending(params string[] properties);
        IEnumerable<TResult> Results { get; }
    }
}
