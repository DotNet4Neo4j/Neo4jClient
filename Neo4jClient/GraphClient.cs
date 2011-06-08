using System;

namespace Neo4jClient
{
    public class GraphClient : IGraphClient
    {
        public NodeReference Create<TNode>(TNode node, params OutgoingRelationship<TNode>[] outgoingRelationships) where TNode : class
        {
            if (node == null)
                throw new ArgumentNullException("node");

            throw new NotImplementedException();
        }
    }
}