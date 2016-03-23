using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neo4jClient
{
    public abstract partial class Relationship
    {
        readonly object data;
        readonly NodeReference otherNode;

        protected Relationship(NodeReference targetNode)
            : this(targetNode, null)
        {
        }

        protected internal Relationship(NodeReference targetNode, object data)
        {
            this.data = data;
            otherNode = targetNode;

            if (targetNode == null)
                throw new ArgumentNullException("targetNode");

            Direction = RelationshipDirection.Automatic;
        }

        public NodeReference OtherNode
        {
            get { return otherNode; }
        }

        public abstract string RelationshipTypeKey { get; }

        public object Data
        {
            get { return data; }
        }

        public RelationshipDirection Direction { get; set; }

        internal static RelationshipDirection DetermineRelationshipDirection(Type baseNodeType, Relationship relationship)
        {
            if (relationship.Direction != RelationshipDirection.Automatic)
                return relationship.Direction;

            var otherNodeType = relationship.OtherNode.NodeType;

            var allowedSourceNodeTypes = GetAllowedNodeTypes(relationship.GetType(), RelationshipEnd.SourceNode).ToArray();
            var isBaseNodeValidAsSourceNode = baseNodeType == null || allowedSourceNodeTypes.Contains(baseNodeType);
            var isOtherNodeValidAsSourceNode = otherNodeType == null || allowedSourceNodeTypes.Contains(otherNodeType);

            var allowedTargetNodeTypes = GetAllowedNodeTypes(relationship.GetType(), RelationshipEnd.TargetNode).ToArray();
            var isBaseNodeValidAsTargetNode = baseNodeType == null || allowedTargetNodeTypes.Contains(baseNodeType);

            if (isBaseNodeValidAsSourceNode &&
                otherNodeType == null)
                return RelationshipDirection.Outgoing;

            if (isBaseNodeValidAsSourceNode &&
                !isBaseNodeValidAsTargetNode)
                return RelationshipDirection.Outgoing;

            if (!isBaseNodeValidAsSourceNode &&
                isBaseNodeValidAsTargetNode &&
                isOtherNodeValidAsSourceNode)
                return RelationshipDirection.Incoming;

            throw new AmbiguousRelationshipDirectionException();
        }

        internal static IEnumerable<Type> GetAllowedNodeTypes(Type relationshipType, RelationshipEnd end)
        {
            Type interfaceType;
            switch (end)
            {
                case RelationshipEnd.SourceNode:
                    interfaceType = typeof (IRelationshipAllowingSourceNode<>);
                    break;
                case RelationshipEnd.TargetNode:
                    interfaceType = typeof (IRelationshipAllowingTargetNode<>);
                    break;
                default:
                    throw new NotSupportedException(string.Format(
                        "The specified relationship end is not supported: {0}", end));
            }

            return relationshipType
                .GetInterfaces()
                .Where(i => i.GetGenericTypeDefinition() == interfaceType)
                .Select(i => i.GetGenericArguments()[0])
                .ToArray();
        }
    }
}