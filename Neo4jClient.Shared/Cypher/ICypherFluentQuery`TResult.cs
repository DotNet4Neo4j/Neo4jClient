using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neo4jClient.Cypher
{
    public interface ICypherFluentQuery<TResult> : ICypherFluentQuery
    {
        IEnumerable<TResult> Results { get; }
        Task<IEnumerable<TResult>> ResultsAsync { get; }

        new ICypherFluentQuery<TResult> Unwind(string collectionName, string columnName);
        new ICypherFluentQuery<TResult> Limit(int? limit);
        new ICypherFluentQuery<TResult> Skip(int? skip);
        new IOrderedCypherFluentQuery<TResult> OrderBy(params string[] properties);
        new IOrderedCypherFluentQuery<TResult> OrderByDescending(params string[] properties);
    }
}
