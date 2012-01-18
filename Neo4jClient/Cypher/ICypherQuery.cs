using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherQuery
    {
        IGraphClient Client { get; }
        string QueryText { get;}
        IDictionary<string, object> QueryParameters { get; }
        IList<string> QueryDeclarations { get; }
    }
}