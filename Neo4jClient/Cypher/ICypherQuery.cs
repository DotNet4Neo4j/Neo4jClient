using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherQuery
    {
        string QueryText { get; }
        IDictionary<string, object> QueryParameters { get; }
    }
}
