using System.Linq;

namespace Neo4jClient.Gremlin
{
    public static class CopySplitStep
    {
        public static IGremlinRelationshipQuery CopySplit(this IGremlinQuery baseQuery, params IGremlinQuery[] queries)
        {
            foreach (var query in queries.Where(query => query.GetType() == typeof(IdentityPipe)))
            {
                ((IdentityPipe) query).Client = query.Client;
            }

            var newQuery = baseQuery.AddCopySplitBlock("._.copySplit({0}, {1})", queries);
            return new GremlinRelationshipEnumerable(newQuery);
        }
    }
}