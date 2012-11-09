using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQuery<TResult> : ICypherFluentQuery
    {
        IEnumerable<TResult> Results { get; }
        Task<IEnumerable<TResult>> ResultsAsync { get; }

        ICypherFluentQuery<TResult> Limit(int? limit);
        ICypherFluentQuery<TResult> Skip(int? skip);
        ICypherFluentQuery<TResult> OrderBy(params string[] properties);
        ICypherFluentQuery<TResult> OrderByDescending(params string[] properties);
    }
}
