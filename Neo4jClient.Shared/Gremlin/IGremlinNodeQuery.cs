using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public interface IGremlinNodeQuery<TNode> : IEnumerable<Node<TNode>>, IGremlinQuery
    {
    }
}