namespace Neo4jClient
{
    public class RelationshipInstance<TData> : RelationshipInstance
        where TData : class, new()
    {
        readonly TData data;

        public RelationshipInstance(
            RelationshipReference reference,
            NodeReference startNodeReference,
            NodeReference endNodeReference,
            TData data)
            : base(reference, startNodeReference, endNodeReference)
        {
            this.data = data;
        }

        public TData Data
        {
            get { return data; }
        }
    }
}
