using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    public interface IGremlinQuery : IAttachedReference
    {
        string QueryText { get;}
        IDictionary<string, object> QueryParameters { get; }
        IList<string> QueryDeclarations { get; }
    }
}