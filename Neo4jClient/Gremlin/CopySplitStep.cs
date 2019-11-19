using System.Linq;

namespace Neo4jClient.Gremlin
{
    public static class CopySplitStep
    {
        public static IGremlinRelationshipQuery CopySplitE(this IGremlinQuery baseQuery, params IGremlinQuery[] queries)
        {
            foreach (var query in queries.Where(query => query.GetType() == typeof(IdentityPipe)))
            {
                ((IdentityPipe) query).Client = query.Client;
            }

            baseQuery = baseQuery.AddCopySplitBlock("._.copySplit({0}, {1})", queries);
            return new GremlinRelationshipEnumerable(baseQuery);
        }

        public static IGremlinNodeQuery<TNode> CopySplitV<TNode>(this IGremlinQuery baseQuery, params IGremlinQuery[] queries)
        {
            foreach (var query in queries.Where(query => query.GetType() == typeof(IdentityPipe)))
            {
                ((IdentityPipe)query).Client = query.Client;
            }

            baseQuery = baseQuery.AddCopySplitBlock("._.copySplit({0}, {1})", queries);
            return new GremlinNodeEnumerable<TNode>(baseQuery);
        }
    }
}