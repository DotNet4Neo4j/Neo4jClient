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
        public CypherFluentQuery(IGraphClient client, CypherQueryBuilder builder)
            : base(client, builder)
        {}

        public ICypherFluentQuery<TResult> Limit(int? limit)
        {
            if (!limit.HasValue) return this;

            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause("LIMIT {0}", limit));
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public ICypherFluentQuery<TResult> Skip(int? skip)
        {
            if (!skip.HasValue) return this;

            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause("SKIP {0}", skip));
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public ICypherFluentQuery<TResult> OrderBy(params string[] properties)
        {
            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause(string.Format("ORDER BY {0}", string.Join(", ", properties))));
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public ICypherFluentQuery<TResult> OrderByDescending(params string[] properties)
        {
            var newBuilder = Builder.CallWriter(w =>
                w.AppendClause(string.Format("ORDER BY {0} DESC", string.Join(", ", properties))));
            return new CypherFluentQuery<TResult>(Client, newBuilder);
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
