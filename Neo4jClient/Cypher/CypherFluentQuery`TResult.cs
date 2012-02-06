using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public class CypherFluentQuery<TResult> :
        CypherFluentQuery,
        ICypherFluentQueryReturned<TResult>
        where TResult : new()
    {
        public CypherFluentQuery(IGraphClient client, CypherQueryBuilder builder)
            : base(client, builder)
        {}

        public ICypherFluentQueryReturned<TResult> Limit(int? limit)
        {
            Builder.Limit = limit;
            return this;
        }

        public IEnumerable<TResult> Results
        {
            get
            {
                return Client.ExecuteGetCypherResults<TResult>(Query);
            }
        }
    }
}
