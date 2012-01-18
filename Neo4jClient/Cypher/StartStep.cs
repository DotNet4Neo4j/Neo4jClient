using System;

namespace Neo4jClient.Cypher
{
    public static class StartStep
    {
        public static ICypherNodeQuery<TNode> Start<TNode>(this ICypherQuery query, string declaration, int[] nodeIds)
        {
            var startText = string.Format("start {0}=", declaration);
            var parameters = new string[nodeIds.Length];
            for (var i = 0; i < nodeIds.Length; i++)
            {
                parameters[i] = string.Format("p{0}", i);
            }

            var newQuery = query.AddStartNodeById(string.Format("{0}node({1})", startText, String.Join(",", parameters)), nodeIds);
            return new CypherNodeEnumerable<TNode>(newQuery);
        }
    }
}
