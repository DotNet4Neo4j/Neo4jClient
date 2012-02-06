using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturned<out TResult> : ICypherFluentQuery
    {
        ICypherFluentQueryReturned<TResult> Limit(int? limit);
        IEnumerable<TResult> Results { get; }
    }
}
