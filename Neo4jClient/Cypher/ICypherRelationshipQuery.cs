using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherRelationshipQuery : IEnumerable<RelationshipInstance>, ICypherQuery
    {
    }
}