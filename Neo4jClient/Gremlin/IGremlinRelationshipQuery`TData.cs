using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public interface IGremlinRelationshipQuery<TData>
        : IEnumerable<RelationshipInstance<TData>>, IGremlinQuery
        where TData : class, new()
    {
    }
}
