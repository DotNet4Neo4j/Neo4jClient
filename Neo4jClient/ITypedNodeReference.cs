using System;

namespace Neo4jClient
{
    public interface ITypedNodeReference
    {
        Type NodeType { get; }
    }
}