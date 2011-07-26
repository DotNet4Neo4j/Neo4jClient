using System;

namespace Neo4jClient.Gremlin
{
    public static class Back
    {
        public static IGremlinNodeQuery<TNode> BackV<TNode>(this IGremlinQuery query, int pipeCount)
        {
            var queryText = String.Format("{0}.back({1})", query.QueryText, pipeCount);
            return new GremlinNodeEnumerable<TNode>(query.Client, queryText);
        }

        public static IGremlinRelationshipQuery BackE(this IGremlinQuery query, int pipeCount)
        {
            var queryText = String.Format("{0}.back({1})", query.QueryText, pipeCount);
            return new GremlinRelationshipEnumerable(query.Client, queryText);
        }
    }
}
