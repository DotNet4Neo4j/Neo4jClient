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
        public CypherFluentQuery(IGraphClient client, QueryWriter writer)
            : base(client, writer)
        {}

        public new ICypherFluentQuery<TResult> Unwind(string collectionName, string columnName)
        {
            return Mutate<TResult>(w => w.AppendClause(string.Format("UNWIND {0} AS {1}", collectionName, columnName)));
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
                w.AppendClause(string.Format("ORDER BY {0}", string.Join(", ", properties))));
        }

        public new IOrderedCypherFluentQuery<TResult> OrderByDescending(params string[] properties)
        {
            return MutateOrdered<TResult>(w =>
                w.AppendClause(string.Format("ORDER BY {0} DESC", string.Join(" DESC, ", properties))));
        }

        public new IOrderedCypherFluentQuery<TResult> ThenBy(params string[] properties)
        {
            return MutateOrdered<TResult>(w =>
                w.AppendToClause(string.Format(", {0}", string.Join(", ", properties))));
        }

        public new IOrderedCypherFluentQuery<TResult> ThenByDescending(params string[] properties)
        {
            return MutateOrdered<TResult>(w =>
                w.AppendToClause(string.Format(", {0} DESC", string.Join(" DESC, ", properties))));
        }

        public IEnumerable<TResult> Results
        {
            get
            {
                return Client.ExecuteGetCypherResults<TResult>(Query);
            }
        }

        public Task<IEnumerable<TResult>> ResultsAsync
        {
            get
            {
                return Client.ExecuteGetCypherResultsAsync<TResult>(Query);
            }
        }
    }
}
