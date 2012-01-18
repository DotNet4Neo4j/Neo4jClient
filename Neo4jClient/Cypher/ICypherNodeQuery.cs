using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherNodeQuery<TNode> : IEnumerable<Node<TNode>>, ICypherQuery
    {
    }
}