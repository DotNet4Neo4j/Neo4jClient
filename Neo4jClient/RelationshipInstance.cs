using System.Diagnostics;

namespace Neo4jClient
{
    [DebuggerDisplay("Relationship {Reference.Id} from node {StartNodeReference.Id} to node {EndNodeReference.Id}")]
    public class RelationshipInstance
    {
        readonly RelationshipReference reference;
        readonly NodeReference startNodeReference;
        readonly NodeReference endNodeReference;

        public RelationshipInstance(
            RelationshipReference reference,
            NodeReference startNodeReference,
            NodeReference endNodeReference)
        {
            this.reference = reference;
            this.startNodeReference = startNodeReference;
            this.endNodeReference = endNodeReference;
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
    }
}
