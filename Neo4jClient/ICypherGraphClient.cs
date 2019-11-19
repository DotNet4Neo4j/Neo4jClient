using System;
using Neo4jClient.Cypher;

namespace Neo4jClient
{
    public interface ICypherGraphClient : IDisposable
    {
        ICypherFluentQuery Cypher { get; }
    }
}