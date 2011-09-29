using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public interface IGremlinRelationshipQuery<TData>
        : IEnumerable<RelationshipInstance<TData>>, IGremlinRelationshipQuery
        where TData : class, new()
    {
    }
}
