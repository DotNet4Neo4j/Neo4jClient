using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.DebugQueryText}")]
    public class CypherFluentQuery<TResult> :
        CypherFluentQuery,
        IOrderedCypherFluentQuery<TResult>
    {
        public CypherFluentQuery(IGraphClient client, QueryWriter writer, bool isWrite = true)
            : base(client, writer, isWrite)
        {}

        public new ICypherFluentQuery<TResult> Unwind(string collectionName, string columnName)
        {
            return Mutate<TResult>(w => w.AppendClause($"UNWIND {collectionName} AS {columnName}"));
        }

        public new ICypherFluentQuery<TResult> Limit(int? limit)
        {
            return limit.HasValue
                ? Mutate<TResult>(w => w.AppendClause("LIMIT {0}", limit))
                : this;
        }

        public new ICypherFluentQuery<TResult> Skip(int? skip)
        {
            return skip.HasValue
                ? Mutate<TResult>(w => w.AppendClause("SKIP {0}", skip))
                : this;
        }

        public new IOrderedCypherFluentQuery<TResult> OrderBy(params string[] properties)
        {
            return MutateOrdered<TResult>(w =>
                w.AppendClause($"ORDER BY {string.Join(", ", properties)}"));
        }

        public new IOrderedCypherFluentQuery<TResult> OrderByDescending(params string[] properties)
        {
            return MutateOrdered<TResult>(w =>
                w.AppendClause($"ORDER BY {string.Join(" DESC, ", properties)} DESC"));
        }

        public new IOrderedCypherFluentQuery<TResult> ThenBy(params string[] properties)
        {
            return MutateOrdered<TResult>(w =>
                w.AppendToClause($", {string.Join(", ", properties)}"));
        }

        public new IOrderedCypherFluentQuery<TResult> ThenByDescending(params string[] properties)
        {
            return MutateOrdered<TResult>(w =>
                w.AppendToClause($", {string.Join(" DESC, ", properties)} DESC"));
        }

        public IEnumerable<TResult> Results => Client.ExecuteGetCypherResults<TResult>(Query);

        public Task<IEnumerable<TResult>> ResultsAsync => Client.ExecuteGetCypherResultsAsync<TResult>(Query);
    }
}
