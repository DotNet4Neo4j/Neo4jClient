namespace Neo4jClient
{
    public class RelationshipInstance<TData> : RelationshipInstance
        where TData : class, new()
    {
        readonly TData data;

        public RelationshipInstance(
            RelationshipReference<TData> reference,
            NodeReference startNodeReference,
            NodeReference endNodeReference,
            TData data)
            : base(reference, startNodeReference, endNodeReference)
        {
            this.data = data;
        }

        public new RelationshipReference<TData> Reference
        {
            get
            {
                var baseReference = base.Reference;
                return new RelationshipReference<TData>(baseReference.Id, ((IAttachedReference) baseReference).Client);
            }
        }

        public TData Data
        {
            get { return data; }
        }
    }
}
