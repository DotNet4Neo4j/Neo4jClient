namespace Neo4jClient.Gremlin
{
    public interface IGremlinQuery
    {
        IGraphClient Client { get; }
        string QueryText { get; }
    }
}