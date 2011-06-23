using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public interface IGremlinEnumerable : IEnumerable<NodeReference>, IGremlinQuery
    {
    }
}