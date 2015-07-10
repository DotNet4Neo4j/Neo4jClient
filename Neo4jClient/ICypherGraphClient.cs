using Neo4jClient.Cypher;

namespace Neo4jClient
{
    public interface ICypherGraphClient
    {
        ICypherFluentQuery Cypher { get; }
    }
}