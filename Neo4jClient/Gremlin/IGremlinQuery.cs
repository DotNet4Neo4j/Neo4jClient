using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public interface IGremlinQuery
    {
        IGraphClient Client { get; }
        string QueryText { get; }
        IDictionary<string, object> QueryParameters { get; }
    }
}