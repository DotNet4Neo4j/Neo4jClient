using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQueryReturned<TResult> : ICypherFluentQuery
    {
        ICypherFluentQueryReturned<TResult> Limit(int? limit);
        ICypherFluentQueryReturned<TResult> Skip(int? skip);
        ICypherFluentQueryReturned<TResult> OrderBy(params string[] properties);
        ICypherFluentQueryReturned<TResult> OrderByDescending(params string[] properties);

        IEnumerable<TResult> Results { get; }
        Task<IEnumerable<TResult>> ResultsAsync { get; }
    }
}
