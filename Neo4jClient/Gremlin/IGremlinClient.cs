using System;

namespace Neo4jClient.Gremlin
{
    [Obsolete("Gremlin support gets dropped with Neo4j 2.0. Please move to equivalent (but much more powerful and readable!) Cypher.")]
    public interface IGremlinClient
    {
        IGremlinQuery V { get; }
        IGremlinQuery E { get; }
    }
}
