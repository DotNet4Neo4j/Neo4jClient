namespace Neo4jClient.Gremlin
{
    public interface IGremlinClient
    {
        IGremlinQuery V { get; }
        IGremlinQuery E { get; }
    }
}
