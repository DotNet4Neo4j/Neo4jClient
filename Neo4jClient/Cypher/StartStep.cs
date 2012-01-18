using System;
using System.Linq;

namespace Neo4jClient.Cypher
{
    public static class StartStep
    {
        /// <summary>
        /// Creates a new Cypher Start Query, all previous steps ignored
        /// </summary>
        public static ICypherNodeQuery<TNode> StartV<TNode>(this ICypherQuery query, string declaration, int[] nodeIds)
        {
            var startText = string.Format("start {0}=", declaration);
            var parameters = new string[nodeIds.Length];
            for (var i = 0; i < nodeIds.Length; i++)
            {
                parameters[i] = string.Format("p{0}", i);
            }

            var newQuery = query.BuildStartById(string.Format("{0}node({1})", startText, String.Join(",", parameters)), nodeIds);
            return new CypherNodeEnumerable<TNode>(newQuery);
        }

        /// <summary>
        /// Creates a new Cypher Start Query, all previous steps ignored
        /// </summary>
        public static ICypherNodeQuery<TNode> StartV<TNode>(this ICypherQuery query, string declaration, NodeReference<TNode>[] nodeReferences)
        {
            var startText = string.Format("start {0}=", declaration);
            var parameters = new string[nodeReferences.Length];
            for (var i = 0; i < nodeReferences.Length; i++)
            {
                parameters[i] = string.Format("p{0}", i);
            }

            var newQuery = query.BuildStartById(string.Format("{0}node({1})", startText, String.Join(",", parameters)), nodeReferences.Select(n => n.Id).ToArray());
            return new CypherNodeEnumerable<TNode>(newQuery);
        }

        /// <summary>
        /// Creates a new Cypher Start Query, all previous steps ignored
        /// </summary>
        public static ICypherRelationshipQuery<TData> StartE<TData>(this ICypherQuery query, string declaration, int[] relatoinshipIds)
            where TData : class, new()
        {
            var startText = string.Format("start {0}=", declaration);
            var parameters = new string[relatoinshipIds.Length];
            for (var i = 0; i < relatoinshipIds.Length; i++)
            {
                parameters[i] = string.Format("p{0}", i);
            }

            var newQuery = query.BuildStartById(string.Format("{0}relationship({1})", startText, String.Join(",", parameters)), relatoinshipIds);
            return new CypherRelationshipEnumerable<TData>(newQuery);
        }

        /// <summary>
        /// Creates a new Cypher Start Query, all previous steps ignored
        /// </summary>
        public static ICypherRelationshipQuery<TData> StartE<TData>(this ICypherQuery query, string declaration, RelationshipReference[] relatoinships)
            where TData : class, new()
        {
            var startText = string.Format("start {0}=", declaration);
            var parameters = new string[relatoinships.Length];
            for (var i = 0; i < relatoinships.Length; i++)
            {
                parameters[i] = string.Format("p{0}", i);
            }

            var newQuery = query.BuildStartById(string.Format("{0}relationship({1})", startText, String.Join(",", parameters)), relatoinships.Select(r => r.Id).ToArray());
            return new CypherRelationshipEnumerable<TData>(newQuery);
        }
    }
}
