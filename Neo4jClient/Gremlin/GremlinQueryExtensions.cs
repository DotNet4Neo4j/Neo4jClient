using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public static class GremlinQueryExtensions
    {
        public static IEnumerable<NodeReference> OutV(this IGremlinQuery query)
        {
            var queryText = string.Format("{0}.{1}", query.QueryText, "outV");
            return new GremlinQueryEnumerable(queryText);
        }
    }
}