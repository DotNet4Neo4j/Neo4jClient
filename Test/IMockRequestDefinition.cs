using System.Collections.Generic;
using RestSharp;

namespace Neo4jClient.Test
{
    public interface IMockRequestDefinition
    {
        Method Method { get; }
        string Resource { get; }
        IEnumerable<Parameter> Parameters { get; }
    }
}
