using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherRelationshipQuery<TData>
        : IEnumerable<RelationshipInstance<TData>>, ICypherQuery
        where TData : class, new()
    {
    }
}
