using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public interface IGremlinRelationshipQuery : IEnumerable<RelationshipInstance>, IGremlinQuery
    {
    }
}