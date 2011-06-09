using System;

namespace Neo4jClient
{
    public abstract class Relationship
    {
        readonly object data;

        protected Relationship(NodeReference targetNode)
            : this(targetNode, null)
        {
        }

        internal Relationship(NodeReference targetNode, object data)
        {
            this.data = data;
            if (targetNode == null)
                throw new ArgumentNullException("targetNode");

            var typedNodeReference = targetNode as ITypedNodeReference;
            if (typedNodeReference != null &&
                targetNode.GetType() != typedNodeReference.NodeType)
            {
                throw new ArgumentException(string.Format(
                    "The type of target node specified is not allowed to be used as the target node of this type of relationship. The target node was of type {0}.",
                    targetNode.GetType().FullName),
                    "targetNode");
            }

            Direction = RelationshipDirection.Automatic;
        }

        public abstract string RelationshipTypeKey { get; }

        public object Data
        {
            get { return data; }
        }

        public RelationshipDirection Direction { get; set; }
    }
}