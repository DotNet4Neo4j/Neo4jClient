using System.Diagnostics;

namespace Neo4jClient
{
    [DebuggerDisplay("Relationship {Reference.Id} of type {TypeKey} from node {StartNodeReference.Id} to node {EndNodeReference.Id}")]
    public class RelationshipInstance
    {
        readonly RelationshipReference reference;
        readonly NodeReference startNodeReference;
        readonly NodeReference endNodeReference;
        readonly string typeKey;

        public RelationshipInstance(
            RelationshipReference reference,
            NodeReference startNodeReference,
            NodeReference endNodeReference,
            string typeKey)
        {
            this.reference = reference;
            this.startNodeReference = startNodeReference;
            this.endNodeReference = endNodeReference;
            this.typeKey = typeKey;
        }

        public RelationshipReference Reference
        {
            get { return reference; }
        }

        public NodeReference StartNodeReference
        {
            get { return startNodeReference; }
        }

        public NodeReference EndNodeReference
        {
            get { return endNodeReference; }
        }

        public string TypeKey
        {
            get { return typeKey; }
        }
    }
}
