using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public static class TableStep
    {
        public static IEnumerable<TResult> Table<TResult>(this IGremlinQuery query)
        {
            var newQuery = query.AddBlock(".table(new Table())");
            return new GremlinProjectionEnumerable<TResult>(newQuery);
        }
    }
}
