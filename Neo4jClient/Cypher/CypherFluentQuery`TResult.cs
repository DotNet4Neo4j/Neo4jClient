using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.DebugQueryText}")]
    public class CypherFluentQuery<TResult> :
        CypherFluentQuery,
        ICypherFluentQuery<TResult>
    {
        public CypherFluentQuery(IGraphClient client, QueryWriter writer)
            : base(client, writer)
        {}

        public ICypherFluentQuery<TResult> Limit(int? limit)
        {
            return limit.HasValue
                ? Mutate<TResult>(w => w.AppendClause("LIMIT {0}", limit))
                : this;
        }

        public ICypherFluentQuery<TResult> Skip(int? skip)
        {
            return skip.HasValue
                ? Mutate<TResult>(w => w.AppendClause("SKIP {0}", skip))
                : this;
        }

        public ICypherFluentQuery<TResult> OrderBy(params string[] properties)
        {
            return Mutate<TResult>(w =>
                w.AppendClause(string.Format("ORDER BY {0}", string.Join(", ", properties))));
        }

        public ICypherFluentQuery<TResult> OrderByDescending(params string[] properties)
        {
            return Mutate<TResult>(w =>
                w.AppendClause(string.Format("ORDER BY {0} DESC", string.Join(", ", properties))));
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
