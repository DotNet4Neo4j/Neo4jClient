namespace Neo4jClient
{
    public class RelationshipInstance<TData> : RelationshipInstance
        where TData : class, new()
    {
        readonly TData data;


        // this constructor is used by DriverDeserializer
        public RelationshipInstance(
            long relationshipId,
            long startNodeId,
            long endNodeId,
            string typeKey,
            TData data)
            : this(new RelationshipReference<TData>(relationshipId), startNodeId, endNodeId, typeKey, data)
        {
        }

        public RelationshipInstance(
            RelationshipReference<TData> reference,
            NodeReference startNodeReference,
            NodeReference endNodeReference,
            string typeKey,
            TData data)
            : base(reference, startNodeReference, endNodeReference, typeKey)
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
