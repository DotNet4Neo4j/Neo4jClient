namespace Neo4jClient
{
    public abstract class Relationship<TData> :
        Relationship
        where TData : class, new()
    {
        protected Relationship(NodeReference targetNode, TData data)
            : base(targetNode, data)
        {
        }
    }
}